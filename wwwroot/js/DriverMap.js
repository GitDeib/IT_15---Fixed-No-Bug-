mapboxgl.accessToken = 'pk.eyJ1IjoiZWx0b25kYXZlZGVvbm8iLCJhIjoiY21hM2VmY3J5MDdqbDJrb2hvZHE2YWlqNSJ9.geC5I_J_EIfffyxTFccFQA';

const map = new mapboxgl.Map({
    container: 'map',
    style: 'mapbox://styles/mapbox/streets-v12',
    center: [125.6131, 7.0659],
    zoom: 10,
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
             <img src="/lib/sedan.png"
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
// Define passenger pickup and destination
const pickupCoords = [125.6131, 7.0659]; // Replace with real pickup
const destinationCoords = [125.6151, 7.0709]; // Replace with real destination

// Add static markers
new mapboxgl.Marker({ color: 'blue' })
    .setLngLat(pickupCoords)
    .setPopup(new mapboxgl.Popup().setText('Passenger Pickup'))
    .addTo(map);

new mapboxgl.Marker({ color: 'red' })
    .setLngLat(destinationCoords)
    .setPopup(new mapboxgl.Popup().setText('Passenger Destination'))
    .addTo(map);

// Create a non-interactive directions route between pickup and destination
const directions = new MapboxDirections({
    accessToken: mapboxgl.accessToken,
    unit: 'metric',
    profile: 'mapbox/driving',
    interactive: false,
    controls: {
        inputs: false,
        instructions: true,
        profileSwitcher: false
    }
});

// Add the directions control to the map
map.addControl(directions, 'top-left');

// Automatically set the route
map.on('load', () => {
    directions.setOrigin(pickupCoords);
    directions.setDestination(destinationCoords);
});
