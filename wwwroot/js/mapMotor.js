mapboxgl.accessToken = 'pk.eyJ1IjoiZWx0b25kYXZlZGVvbm8iLCJhIjoiY21hM2VmY3J5MDdqbDJrb2hvZHE2YWlqNSJ9.geC5I_J_EIfffyxTFccFQA';

const map = new mapboxgl.Map({
    container: 'map',
    style: 'mapbox://styles/mapbox/streets-v12',
    center: [125.6131, 7.0659],
    zoom: 15,
    pitch: 45,
    bearing: -17.6,
    antialias: true
});

// Add 3D buildings after the map loads
map.on('load', () => {
    const layers = map.getStyle().layers;
    const labelLayerId = layers.find(
        layer => layer.type === 'symbol' && layer.layout['text-field']
    )?.id;

    map.addLayer({
        'id': '3d-buildings',
        'source': 'composite',
        'source-layer': 'building',
        'filter': ['==', 'extrude', 'true'],
        'type': 'fill-extrusion',
        'minzoom': 15,
        'paint': {
            'fill-extrusion-color': '#aaa',
            'fill-extrusion-height': [
                'interpolate', ['linear'], ['zoom'],
                15, 0,
                15.05, ['get', 'height']
            ],
            'fill-extrusion-base': [
                'interpolate', ['linear'], ['zoom'],
                15, 0,
                15.05, ['get', 'min_height']
            ],
            'fill-extrusion-opacity': 0.6
        }
    }, labelLayerId);
});

// Create marker using Bootstrap badge and icon
function createUserMarker(lng, lat) {
    const el = document.createElement('div');
    el.innerHTML = `
        <div class="d-flex flex-column align-items-center">
            <span class="badge bg-dark mb-1">You're here</span>
            <div class="rounded-circle bg-primary border border-white" style="width: 20px; height: 20px;"></div>
        </div>
    `;
    return new mapboxgl.Marker(el).setLngLat([lng, lat]).addTo(map);
}

// Create driver marker (car icon)
function createDriverMarker(lng, lat) {
    const el = document.createElement('div');
    el.innerHTML = `
    <div class="d-flex flex-column align-items-center">
            <span class="badge bg-dark mb-1">Your Driver</span>
             <img src="/lib/motorbike.png"
             alt="Car Icon"
             style="width: 50px; height: 50px;" />
        </div>
    `;
    return new mapboxgl.Marker(el).setLngLat([lng, lat]).addTo(map);
}

// Track user location
if (navigator.geolocation) {
    navigator.geolocation.watchPosition(position => {
        const lng = position.coords.longitude;
        const lat = position.coords.latitude;

        map.flyTo({
            center: [lng, lat],
            speed: 1.2,
            zoom: 16
        });

        if (!window.userMarker) {
            window.userMarker = createUserMarker(lng, lat);
        } else {
            window.userMarker.setLngLat([lng, lat]);
        }

        // Show static driver marker nearby
        if (!window.driverMarker) {
            const driverLng = lng + 0.0015;
            const driverLat = lat + 0.0010;
            window.driverMarker = createDriverMarker(driverLng, driverLat);
        }

    }, error => {
        console.error('GPS Error:', error);
    }, {
        enableHighAccuracy: true,
        timeout: 5000,
        maximumAge: 0
    });
} else {
    console.warn('Geolocation not supported by this browser.');
}

// ========================
// Directions Setup
// ========================

// Create a Directions client
const directions = new MapboxDirections({
    accessToken: mapboxgl.accessToken,
    unit: 'metric',
    profile: 'mapbox/driving',
    interactive: false,
    controls: {
        inputs: false,
        instructions: true
    }
});
map.addControl(directions, 'bottom-left');

// Event listener to set route when inputs change
document.getElementById('pickup').addEventListener('change', () => {
    updateDirections();
});
document.getElementById('dropoff').addEventListener('change', () => {
    updateDirections();
});

function updateDirections() {
    const pickup = document.getElementById('pickup').value;
    const dropoff = document.getElementById('dropoff').value;

    if (pickup && dropoff) {
        // Use Mapbox Geocoding API to get coordinates
        Promise.all([
            geocodeLocation(pickup),
            geocodeLocation(dropoff)
        ]).then(([pickupCoords, dropoffCoords]) => {
            directions.setOrigin(pickupCoords);
            directions.setDestination(dropoffCoords);

            // Call Directions API to get distance and duration
            const url = `https://api.mapbox.com/directions/v5/mapbox/driving/${pickupCoords[0]},${pickupCoords[1]};${dropoffCoords[0]},${dropoffCoords[1]}?geometries=geojson&access_token=${mapboxgl.accessToken}`;
            
            fetch(url)
                .then(response => response.json())
                .then(data => {
                    if (data.routes.length > 0) {
                        const route = data.routes[0];
                        const distance = (route.distance / 1000).toFixed(2); // km
                        const duration = Math.ceil(route.duration / 60); // minutes

                        // Update UI with distance and duration
                        document.getElementById('distanceDisplay').innerText = `${distance} km`;
                        document.getElementById('timeDisplay').innerText = `${duration} min`;

                        // You can also calculate the fare based on these values if needed
                        const baseFare = 50;
                        const perKm = 12;
                        const perMin = 3;
                        const totalFare = baseFare + (distance * perKm) + (duration * perMin);
                        document.getElementById('totalFare').innerText = totalFare.toFixed(2);
                    }
                })
                .catch(error => console.error('Error fetching Directions API:', error));
        }).catch(err => {
            console.error('Geocoding error:', err);
            alert('Unable to find locations. Try more specific addresses.');
        });
    }
}

function geocodeLocation(query) {
    const url = `https://api.mapbox.com/geocoding/v5/mapbox.places/${encodeURIComponent(query)}.json?access_token=${mapboxgl.accessToken}`;
    return fetch(url)
        .then(res => res.json())
        .then(data => {
            if (data.features.length > 0) {
                return data.features[0].center;
            } else {
                throw new Error('Location not found');
            }
        });
}
