    const accessToken = 'pk.eyJ1IjoiZWx0b25kYXZlZGVvbm8iLCJhIjoiY21hM2VmY3J5MDdqbDJrb2hvZHE2YWlqNSJ9.geC5I_J_EIfffyxTFccFQA';
    mapboxgl.accessToken = accessToken;

    const map = new mapboxgl.Map({
        container: 'map',
    style: 'mapbox://styles/mapbox/streets-v12',
    center: [125.6131, 7.0731],
    zoom: 12
        });

    const directions = new MapboxDirections({
        accessToken: accessToken,
    unit: 'metric',
    profile: 'mapbox/driving',
    interactive: false,
    controls: {
        inputs: false,
    instructions: true,
    profileSwitcher: false
            }
        });

    map.addControl(directions, 'top-left');

    let pickupMarker = null;
    let dropoffMarker = null;

    function addPickupPin() {
            if (pickupMarker) pickupMarker.remove();

    const center = map.getCenter();
    pickupMarker = new mapboxgl.Marker({draggable: true, color: 'green' })
    .setLngLat(center)
    .addTo(map)
                .on('dragend', () => updateRoute());

            // Do not call updateRoute() here, only after dragend.
        }

    function addDropoffPin() {
            if (dropoffMarker) dropoffMarker.remove();

    const center = map.getCenter();
    dropoffMarker = new mapboxgl.Marker({draggable: true, color: 'red' })
    .setLngLat(center)
    .addTo(map)
                .on('dragend', () => updateRoute());

            // Do not call updateRoute() here, only after dragend.
        }

    function updateRoute() {
            if (pickupMarker && dropoffMarker) {
                const pickupLngLat = pickupMarker.getLngLat();
    const dropoffLngLat = dropoffMarker.getLngLat();

    directions.setOrigin(pickupLngLat.toArray());
    directions.setDestination(dropoffLngLat.toArray());
    reverseGeocode(pickupLngLat, 'pickup');
    reverseGeocode(dropoffLngLat, 'dropoff');
            }
        }

    function reverseGeocode(lngLat, type) {
        fetch(`https://api.mapbox.com/geocoding/v5/mapbox.places/${lngLat.lng},${lngLat.lat}.json?access_token=${accessToken}`)
            .then(res => res.json())
            .then(data => {
                const address = data.features[0]?.place_name || 'Address not found';
                document.getElementById(type + 'Address').value = address;
            })
            .catch(() => {
                document.getElementById(type + 'Address').value = 'Error retrieving address';
            });
        }
