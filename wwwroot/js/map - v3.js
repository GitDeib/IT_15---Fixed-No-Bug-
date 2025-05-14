const accessToken = 'pk.eyJ1IjoiZWx0b25kYXZlZGVvbm8iLCJhIjoiY21hM2VmY3J5MDdqbDJrb2hvZHE2YWlqNSJ9.geC5I_J_EIfffyxTFccFQA';
mapboxgl.accessToken = accessToken;

const map = new mapboxgl.Map({
    container: 'map',
    style: 'mapbox://styles/mapbox/streets-v12',
    center: [125.6131, 7.0731],
    zoom: 14,
    pitch: 45,
    bearing: -17.6,
    antialias: true
});

// 3D Buildings Layer
map.on('load', () => {
    const layers = map.getStyle().layers;
    const labelLayerId = layers.find(layer => layer.type === 'symbol' && layer.layout['text-field'])?.id;

    map.addLayer({
        id: '3d-buildings',
        source: 'composite',
        'source-layer': 'building',
        filter: ['==', 'extrude', 'true'],
        type: 'fill-extrusion',
        minzoom: 15,
        paint: {
            'fill-extrusion-color': '#aaa',
            'fill-extrusion-height': [
                'interpolate', ['linear'], ['zoom'], 15, 0, 15.05, ['get', 'height']
            ],
            'fill-extrusion-base': [
                'interpolate', ['linear'], ['zoom'], 15, 0, 15.05, ['get', 'min_height']
            ],
            'fill-extrusion-opacity': 0.6
        }
    }, labelLayerId);
});

// Mapbox Directions setup
const directions = new MapboxDirections({
    accessToken,
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
            const lngLat = dropoffMarker.getLngLat();
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

let currentFareRates = {
    baseFare: 0,
    perKilometerRate: 0,
    perMinuteRate: 0
};

directions.on('route', function (e) {
    if (e.route && e.route.length > 0) {
        const route = e.route[0];
        const distanceKm = (route.distance / 1000).toFixed(2);
        const durationMin = (route.duration / 60).toFixed(2);

        const totalFare =
            parseFloat(currentFareRates.baseFare) +
            parseFloat(currentFareRates.perKilometerRate) * distanceKm +
            parseFloat(currentFareRates.perMinuteRate) * durationMin;

        document.getElementById('distanceDisplay').innerText = `${distanceKm} km`;
        document.getElementById('timeDisplay').innerText = `${durationMin} minutes`;
        document.getElementById('totalFare').innerText = `₱${totalFare.toFixed(2)}`;
    }
});

function reverseGeocode(lngLat, type) {
    fetch(`https://api.mapbox.com/geocoding/v5/mapbox.places/${lngLat.lng},${lngLat.lat}.json?access_token=${accessToken}`)
        .then(res => res.json())
        .then(data => {
            const address = data.features[0]?.place_name || 'Unknown';
            document.getElementById(type).value = address;

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

    }, error => {
        console.error('GPS Error:', error);
    }, {
        enableHighAccuracy: true,
        timeout: 5000,
        maximumAge: 0
    });
}

function checkIfReadyToProceed() {
    const pickup = document.getElementById('pickup').value.trim();
    const dropoff = document.getElementById('dropoff').value.trim();
    const nextBtn = document.getElementById('nextBtn');
    nextBtn.disabled = !(pickup && dropoff);
}

function openSidebar() {
    const pickup = document.getElementById('pickup').value;
    const dropoff = document.getElementById('dropoff').value;

    if (pickup && dropoff) {
        const sidebarEl = document.getElementById('sidepass');
        const sidebar = bootstrap.Offcanvas.getOrCreateInstance(sidebarEl); // Ensure it's an Offcanvas component
        sidebar.show();
    }
}

document.getElementById('nextBtn').addEventListener('click', openSidebar);

document.getElementById("rideType").addEventListener("change", function () {
    var seatType = this.value;
    fetchFareData(seatType);
});

function fetchFareData(seatType) {
    fetch('/Home/GetFareDetails?seatType=' + seatType)
        .then(response => response.json())
        .then(data => {
            if (data.error) {
                alert(data.error);
                return;
            }

            currentFareRates = {
                baseFare: parseFloat(data.baseFare),
                perKilometerRate: parseFloat(data.perKilometerRate),
                perMinuteRate: parseFloat(data.perMinuteRate)
            };

            document.getElementById("baseFare").textContent = '₱ ' + data.baseFare;
            document.getElementById("perKm").textContent = '₱ ' + data.perKilometerRate + '/km';
            document.getElementById("perMinute").textContent = '₱ ' + data.perMinuteRate + '/min';

            // Recalculate if route exists
            updateRoute();
        })
        .catch(error => {
            console.error('Error fetching fare data:', error);
            alert('An error occurred while fetching the fare data.');
        });
}

const vehicleTypeSelect = document.getElementById('Typevehicle');
const seatTypeSelect = document.getElementById('rideType');
const fareSettingsInput = document.getElementById("FareSettingsIdInput");

// Update seat options based on vehicle type
vehicleTypeSelect.addEventListener('change', function () {
    seatTypeSelect.innerHTML = '<option disabled selected>Select Type</option>';

    if (vehicleTypeSelect.value === 'Car') {
        seatTypeSelect.innerHTML += '<option value="4-seater">4-seater</option><option value="6-seater">6-seater</option>';
    } else if (vehicleTypeSelect.value === 'Motorcycle') {
        seatTypeSelect.innerHTML += '<option value="1-seater">1-seater</option>';
    }

    // Reset fare setting input
    fareSettingsInput.value = "";
});

// Update FareSettings ID when a seat type is selected
seatTypeSelect.addEventListener('change', function () {
    const seatType = seatTypeSelect.value;

    const fareSettingsMap = {
        '4-seater': 1,
        '6-seater': 2,
        '1-seater': 3
    };

    fareSettingsInput.value = fareSettingsMap[seatType] || "";
});

function submitBooking() {
    const vehicleTypeSelect = document.getElementById("Typevehicle");
    const seatTypeSelect = document.getElementById("rideType");

    const vehicleTypeError = document.getElementById("vehicleTypeError");
    const seatTypeError = document.getElementById("seatTypeError");

    let hasError = false;

    // Reset error messages
    vehicleTypeError.classList.add("d-none");
    seatTypeError.classList.add("d-none");
    vehicleTypeSelect.classList.remove("is-invalid");
    seatTypeSelect.classList.remove("is-invalid");

    // Validation
    if (vehicleTypeSelect.selectedIndex === 0) {
        vehicleTypeError.classList.remove("d-none");
        vehicleTypeSelect.classList.add("is-invalid");
        hasError = true;
    }
    if (seatTypeSelect.selectedIndex === 0) {
        seatTypeError.classList.remove("d-none");
        seatTypeSelect.classList.add("is-invalid");
        hasError = true;
    }

    if (hasError) return; // Stop form submission if any errors

    // Proceed to set hidden inputs and submit the form
    const pickup = document.getElementById("pickupLocationDisplay").textContent;
    const dropoff = document.getElementById("dropoffLocationDisplay").textContent;
    const pickupLat = document.getElementById("pickupLat").textContent;
    const pickupLng = document.getElementById("pickupLng").textContent;
    const dropoffLat = document.getElementById("dropoffLat").textContent;
    const dropoffLng = document.getElementById("dropoffLng").textContent;
    const distance = document.getElementById("distanceDisplay").textContent.replace(/[^0-9.]/g, "");
    const time = document.getElementById("timeDisplay").textContent.replace(/[^0-9.]/g, "");
    const fare = document.getElementById("totalFare").textContent.replace(/[₱,]/g, "");
    const fareSettingId = document.getElementById("FareSettingsIdInput").value;

    document.getElementById("PickupLocationInput").value = pickup;
    document.getElementById("DropoffLocationInput").value = dropoff;
    document.getElementById("PickupLatInput").value = pickupLat;
    document.getElementById("PickupLngInput").value = pickupLng;
    document.getElementById("DropoffLatInput").value = dropoffLat;
    document.getElementById("DropoffLngInput").value = dropoffLng;
    document.getElementById("DistanceInput").value = distance;
    document.getElementById("TimeInput").value = time;
    document.getElementById("FareInput").value = fare;
    document.getElementById("VehicleTypeInput").value = vehicleTypeSelect.value;
    document.getElementById("VehicleSeatInput").value = seatTypeSelect.value;
    document.getElementById("FareSettingsIdInput").value = fareSettingId;

    document.getElementById("bookingForm").submit();
}

