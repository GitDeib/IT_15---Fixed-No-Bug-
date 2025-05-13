const accessToken = 'pk.eyJ1IjoiZWx0b25kYXZlZGVvbm8iLCJhIjoiY21hM2VmY3J5MDdqbDJrb2hvZHE2YWlqNSJ9.geC5I_J_EIfffyxTFccFQA';
mapboxgl.accessToken = accessToken;

const map = new mapboxgl.Map({
    container: 'map',
    style: 'mapbox://styles/mapbox/streets-v12', /*mapbox://styles/mapbox/navigation-night-v1   mapbox://styles/mapbox/dark-v11*/
    center: [125.6131, 7.0731],
    zoom: 14,
    pitch: 45,
    bearing: -17.6,
    antialias: true
});

//3D Building
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

// Add Directions control
const directions = new MapboxDirections({
    accessToken: accessToken,
    unit: 'metric',
    profile: 'mapbox/driving',
    interactive: false,
    controls: {
        inputs: false,
        instructions: false,
        profileSwitcher: false
    }
});
map.addControl(directions, 'top-left');

let pickupMarker = null;
let dropoffMarker = null;

function addPickupPin() {
    if (pickupMarker) pickupMarker.remove();
    const center = map.getCenter();
    pickupMarker = new mapboxgl.Marker({ draggable: true, color: 'blue' })
        .setLngLat(center)
        .addTo(map)
        .on('dragend', () => {
            const lngLat = pickupMarker.getLngLat();
            reverseGeocode(lngLat, 'pickup');
            updateRoute();
        });
    reverseGeocode(center, 'pickup');
}

function addDropoffPin() {
    if (dropoffMarker) dropoffMarker.remove();
    const center = map.getCenter();
    dropoffMarker = new mapboxgl.Marker({ draggable: true, color: 'red' })
        .setLngLat(center)
        .addTo(map)
        .on('dragend', () => {
            const lngLat = dropoffMarker.getLngLat(); // ✅ Get updated location
            reverseGeocode(lngLat, 'dropoff');
            updateRoute();
        });
    reverseGeocode(center, 'dropoff');
}

function updateRoute() {
    if (pickupMarker && dropoffMarker) {
        const pickupLngLat = pickupMarker.getLngLat();
        const dropoffLngLat = dropoffMarker.getLngLat();
        directions.setOrigin(pickupLngLat.toArray());
        directions.setDestination(dropoffLngLat.toArray());
    }
}
directions.on('route', function (e) {
    if (e.route && e.route.length > 0) {
        const route = e.route[0];
        const distanceKm = (route.distance / 1000).toFixed(2); // Convert distance to kilometers
        const durationMin = (route.duration / 60).toFixed(2); // Convert duration to minutes

        // Display distance and time in your sidebar or any element
        document.getElementById('distanceDisplay').innerText = `${distanceKm} km`;
        document.getElementById('timeDisplay').innerText = `${durationMin} minutes`;

        // Display the total fare
        document.getElementById('totalFare').innerText = `₱${totalFare.toFixed(2)}`;
    }
});



function reverseGeocode(lngLat, type) {
    fetch(`https://api.mapbox.com/geocoding/v5/mapbox.places/${lngLat.lng},${lngLat.lat}.json?access_token=${accessToken}`)
        .then(res => res.json())
        .then(data => {
            const address = data.features[0]?.place_name || 'Unknown';
            document.getElementById(type).value = address;

            // Set display spans
            if (type === 'pickup') {
                document.getElementById('pickupLocationDisplay').innerText = address;
                document.getElementById('pickupLat').innerText = lngLat.lat.toFixed(6);
                document.getElementById('pickupLng').innerText = lngLat.lng.toFixed(6);
            } else if (type === 'dropoff') {
                document.getElementById('dropoffLocationDisplay').innerText = address;
                document.getElementById('dropoffLat').innerText = lngLat.lat.toFixed(6);
                document.getElementById('dropoffLng').innerText = lngLat.lng.toFixed(6);
            }

            checkIfReadyToProceed();
        })
        .catch(() => {
            document.getElementById(type).value = 'Error getting address';
        });
}

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



//Sidepass

function checkIfReadyToProceed() {
    const pickup = document.getElementById('pickup').value.trim();
    const dropoff = document.getElementById('dropoff').value.trim();
    const nextBtn = document.getElementById('nextBtn');
    nextBtn.disabled = !(pickup && dropoff);
}


// Function to open sidebar when conditions are met
function openSidebar() {
    const pickup = document.getElementById('pickup').value;
    const dropoff = document.getElementById('dropoff').value;

    if (pickup && dropoff) {
        // Open the sidebar if both fields are filled
        const sidebar = new bootstrap(document.getElementById('sidepass'));
        sidebar.show();
    }
}

// Attach click event to the "Next" button to trigger sidebar
document.getElementById('nextBtn').addEventListener('click', openSidebar);