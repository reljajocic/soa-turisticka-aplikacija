<template>
  <div class="container">
    <h2>Position Simulator</h2>
    <p class="desc">Click on the map to simulate your GPS location.</p>
    
    <div class="simulator-card card">
        <div id="sim-map"></div>
        
        <div class="status-panel">
            <div v-if="currentPos" class="pos-info">
                <span class="label">Current Location:</span>
                <code class="coords">{{ currentPos.lat.toFixed(5) }}, {{ currentPos.lng.toFixed(5) }}</code>
                <span class="badge">Active</span>
            </div>
            <div v-else class="pos-info">
                <span class="label">Location not set.</span>
            </div>
        </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'
import api from '@/services/api'
import icon from 'leaflet/dist/images/marker-icon.png'
import iconShadow from 'leaflet/dist/images/marker-shadow.png'

const currentPos = ref(null)
let map = null
let marker = null

// Fix za Leaflet ikonice
let DefaultIcon = L.icon({
    iconUrl: icon,
    shadowUrl: iconShadow,
    iconSize: [25, 41],
    iconAnchor: [12, 41],
    popupAnchor: [1, -34]
});
L.Marker.prototype.options.icon = DefaultIcon;

onMounted(async () => {
    // 1. Inicijalizacija mape (Centar Novog Sada)
    map = L.map('sim-map').setView([45.255, 19.845], 14);
    
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap'
    }).addTo(map);

    // 2. Klik na mapu -> Šalje podatke na backend
    map.on('click', async (e) => {
        await updatePosition(e.latlng.lat, e.latlng.lng)
    })

    // 3. Učitaj trenutnu poziciju sa backenda (ako postoji)
    loadPosition()
})

const loadPosition = async () => {
    try {
        const res = await api.get('/position')
        if(res.data) {
            // Backend vraća objekat { lat: ..., lon: ... } ili { latitude: ..., longitude: ... }
            // Prilagodi zavisno od toga kako ti UserPosition model vraća JSON (camelCase verovatno)
            const lat = res.data.lat || res.data.latitude
            const lng = res.data.lon || res.data.longitude
            
            if(lat && lng) {
                setMarker(lat, lng)
            }
        }
    } catch (e) {
        console.log("Position not set yet.")
    }
}

const updatePosition = async (lat, lng) => {
    try {
        // Šaljemo SetPosDto: { Latitude, Longitude }
        await api.post('/position', { latitude: lat, longitude: lng })
        setMarker(lat, lng)
    } catch (e) {
        alert("Failed to update position. Make sure Gateway is configured!")
        console.error(e)
    }
}

const setMarker = (lat, lng) => {
    currentPos.value = { lat, lng }
    
    if (marker) {
        marker.setLatLng([lat, lng])
    } else {
        marker = L.marker([lat, lng]).addTo(map)
    }
    marker.bindPopup("<b>You are here!</b>").openPopup()
    map.panTo([lat, lng])
}
</script>

<style scoped>
.container { text-align: center; max-width: 800px; margin: 30px auto; }
.desc { color: #666; margin-bottom: 20px; font-size: 1.1rem; }

.simulator-card {
    background: white;
    border-radius: 12px;
    box-shadow: 0 4px 15px rgba(0,0,0,0.1);
    overflow: hidden;
    border: none;
}

#sim-map {
    height: 500px;
    width: 100%;
    z-index: 1;
}

.status-panel {
    padding: 15px;
    background: #f8f9fa;
    border-top: 1px solid #eee;
}

.pos-info {
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 15px;
    font-size: 1.1rem;
}

.label { font-weight: 600; color: #2c3e50; }
.coords { background: #e9ecef; padding: 4px 8px; border-radius: 4px; font-family: monospace; color: #c0392b; }
.badge { background: #28a745; color: white; padding: 4px 10px; border-radius: 12px; font-size: 0.8rem; text-transform: uppercase; font-weight: bold; }
</style>