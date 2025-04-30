// Mapbox Public Access Key
mapboxgl.accessToken = 'pk.eyJ1IjoiZWx0b25kYXZlZGVvbm8iLCJhIjoiY21hM2VmY3J5MDdqbDJrb2hvZHE2YWlqNSJ9.geC5I_J_EIfffyxTFccFQA';

// Initializing Map
var map = new mapboxgl.Map({
    container: 'map',
    style: 'mapbox://styles/eltondavedeono/cma3esxlj000m01rf7eq61e8o',
    zoom: 15,  // Adjust zoom level as needed
});

// Directions control
var directions = new MapboxDirections({
    accessToken: mapboxgl.accessToken
});

// Adding Directions control to map
map.addControl(directions, 'top-left');

// Adding navigation control on Map
map.addControl(new mapboxgl.NavigationControl(), 'bottom-right');

// Map Loaded
map.on('load', function () {

    // Function to set Directions when both Pickup and Dropoff locations are filled
    function setDirections() {
        var pickupLocation = $('#pickup-location').val();
        var dropoffLocation = $('#dropoff-location').val();

        // Ensure both pickup and dropoff locations are provided
        if (pickupLocation && dropoffLocation) {
            directions.setOrigin(pickupLocation);
            directions.setDestination(dropoffLocation);
        }
    }

    // Set Directions when input changes
    $('#pickup-location, #dropoff-location').on('input', function () {
        setDirections();
    });

    // Initial Directions Setup
    setDirections();

    // Get user's current location and center the map on it
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
            var userLng = position.coords.longitude;
            var userLat = position.coords.latitude;

            // Center the map on the user's location
            map.setCenter([userLng, userLat]);

            // Optional: Add a marker at the user's location
            new mapboxgl.Marker()
                .setLngLat([userLng, userLat])
                .addTo(map);
        }, function (error) {
            console.error('Error getting GPS location:', error);
        });
    } else {
        console.error('Geolocation is not supported by this browser.');
    }
});
