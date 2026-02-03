<template>
  <div class="container">
    <div v-if="loading" class="loading-screen">
        <h2>Starting your adventure...</h2>
        <div class="spinner"></div>
    </div>
    
    <div v-else class="active-tour-wrapper">
      <div class="status-card card">
        <div class="header">
            <h2>Active Tour <span class="pulse">●</span></h2>
            <div class="live-badge">LIVE TRACKING</div>
        </div>
        
        <h3>{{ tour?.name }}</h3>
        
        <div class="target-display" v-if="displayPoint">
            <div class="target-image-wrapper">
                <img :src="displayPoint.imageUrl || displayPoint.image || 'https://via.placeholder.com/400x250?text=No+Image'" alt="Target" />
                <div class="target-overlay" v-if="!isFinished">
                    <span class="target-label">Current Goal</span>
                    <span class="target-name">{{ displayPoint.name }}</span>
                </div>
            </div>
            <div class="target-content">
                <p class="target-description">{{ displayPoint.description }}</p>
            </div>
        </div>

        <div class="info-box">
            <div v-if="isFinished" class="finished-msg">
                <i class="fa fa-trophy fa-3x"></i> 
                <h4>Tour Completed!</h4>
                <p>Congratulations, you've reached all points.</p>
            </div>
            
            <div v-else class="distance-section">
                <p class="label">Distance to next stop:</p>
                <h1 class="dist" :class="{'near': distanceKm < 0.05}">{{ distanceDisplay }}</h1>
                <div v-if="distanceKm < 0.05" class="arrival-alert">
                    <i class="fa fa-check-circle"></i> You are at the location!
                </div>
                <small class="hint">
                    <i class="fa fa-info-circle"></i> 
                    Check <b>Simulator</b> to update your GPS position.
                </small>
            </div>
        </div>

        <div class="progress-section">
            <div class="progress-bar-bg">
                <div class="progress-bar-fill" :style="{ width: progressPercent + '%' }"></div>
            </div>
            <p class="steps-text">{{ progressIndex }} / {{ tour?.keypoints?.length }} checkpoints visited</p>
        </div>

        <div class="checkpoints-log" v-if="arrivalTimes.length > 0">
            <h4>Activity Log:</h4>
            <ul>
                <li v-for="(time, idx) in arrivalTimes" :key="idx">
                    <i class="fa fa-check-circle"></i> 
                    {{ tour.keypoints[idx]?.name }} - {{ new Date(time).toLocaleTimeString() }}
                </li>
            </ul>
        </div>

        <div class="actions">
            <button v-if="!isFinished" @click="abandon" class="btn btn-abandon">
                <i class="fa fa-times"></i> Abandon Tour
            </button>
            <button v-else @click="$router.push('/my-tours')" class="btn btn-finish">
                Finish & Exit
            </button>
        </div>
      </div>

      <div class="map-card card">
        <div id="active-map"></div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, onUnmounted, ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useTourStore } from '@/stores/tours'
import { useExecutionStore } from '@/stores/execution'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'

const route = useRoute()
const router = useRouter()
const tourStore = useTourStore()
const execStore = useExecutionStore()

const executionId = route.params.id
const tourId = route.query.tourId

const tour = ref(null)
const loading = ref(true)
const progressIndex = ref(0)
const distanceKm = ref(null)
const isFinished = ref(false)
const arrivalTimes = ref([])
let pollingInterval = null
let map = null
let markers = []

// --- ICONS SETUP ---
const DefaultIcon = L.icon({
    iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-blue.png',
    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
    iconSize: [25, 41], iconAnchor: [12, 41]
});
const GreenIcon = L.icon({
    iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-green.png',
    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
    iconSize: [25, 41], iconAnchor: [12, 41]
});
const PulseIcon = L.icon({
    iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png',
    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
    iconSize: [30, 48], iconAnchor: [15, 48] // Malo veći za cilj
});

const displayPoint = computed(() => {
    if(!tour.value?.keypoints) return null
    return isFinished.value 
        ? tour.value.keypoints[tour.value.keypoints.length - 1] 
        : tour.value.keypoints[progressIndex.value]
})

const progressPercent = computed(() => {
    if(!tour.value?.keypoints?.length) return 0
    return (progressIndex.value / tour.value.keypoints.length) * 100
})

const distanceDisplay = computed(() => {
    if(distanceKm.value === null || distanceKm.value === undefined) return "Calculating..."
    if(distanceKm.value < 1) return (distanceKm.value * 1000).toFixed(0) + " m"
    return distanceKm.value.toFixed(2) + " km"
})

onMounted(async () => {
    try {
        tour.value = await tourStore.getTour(tourId)
        await pollBackend()
        loading.value = false
        setTimeout(initMap, 100)
        pollingInterval = setInterval(pollBackend, 10000) // Specifikacija: 10s
    } catch (e) {
        router.push('/my-tours')
    }
})

onUnmounted(() => { if(pollingInterval) clearInterval(pollingInterval) })

const initMap = () => {
    if(!tour.value?.keypoints?.length) return
    map = L.map('active-map').setView([tour.value.keypoints[0].lat, tour.value.keypoints[0].lon], 15)
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(map)
    updateMarkers()
}

const updateMarkers = () => {
    if(!map) return
    markers.forEach(m => map.removeLayer(m))
    markers = []

    tour.value.keypoints.forEach((kp, i) => {
        let icon = DefaultIcon
        let status = "Future Stop"
        
        if (i < progressIndex.value) {
            icon = GreenIcon
            status = "Visited"
        } else if (i === progressIndex.value && !isFinished.value) {
            icon = PulseIcon
            status = "CURRENT GOAL"
        }

        const m = L.marker([kp.lat, kp.lon], { icon }).addTo(map)
            .bindPopup(`<b>${status}:</b> ${kp.name}`)
        
        if (i === progressIndex.value) m.openPopup()
        markers.push(m)
    })
}

const pollBackend = async () => {
    if(isFinished.value) return
    try {
        const res = await execStore.pollPosition(executionId)
        distanceKm.value = res.distanceKm
        if (res.arrivalTimes) arrivalTimes.value = res.arrivalTimes
        
        if (res.progress !== undefined && res.progress > progressIndex.value) {
            progressIndex.value = res.progress
            updateMarkers()
        }
        
        if (res.completed || progressIndex.value >= tour.value.keypoints.length) {
            isFinished.value = true
            clearInterval(pollingInterval)
        }
    } catch (e) { console.error(e) }
}

const abandon = async () => {
    if(confirm("Abandon tour? Your progress will be saved.")) {
        await execStore.finishTour(executionId, false)
        router.push('/my-tours')
    }
}
</script>

<style scoped>
.active-tour-wrapper { display: flex; gap: 25px; height: calc(100vh - 140px); margin-top: 20px; }
.status-card { flex: 1; min-width: 400px; display: flex; flex-direction: column; padding: 25px; border-radius: 15px; }

.header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.live-badge { background: #cc072a; color: white; padding: 4px 10px; border-radius: 5px; font-size: 0.75rem; font-weight: bold; animation: pulse 2s infinite; }
.pulse { color: #cc072a; animation: blink 1s infinite; }

.target-image-wrapper { position: relative; width: 100%; height: 220px; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.1); }
.target-image-wrapper img { width: 100%; height: 100%; object-fit: cover; }
.target-overlay { position: absolute; bottom: 0; left: 0; width: 100%; padding: 15px; background: linear-gradient(transparent, rgba(0,0,0,0.8)); color: white; }
.target-label { display: block; font-size: 0.7rem; text-transform: uppercase; opacity: 0.8; }
.target-name { font-size: 1.2rem; font-weight: bold; }

.target-content { padding: 15px 0; }
.target-description { font-size: 0.95rem; color: #555; line-height: 1.5; }

.info-box { text-align: center; background: #f8f9fa; border-radius: 12px; padding: 20px; margin-bottom: 20px; border: 1px solid #eee; }
.dist { font-size: 3.2rem; color: #2c3e50; margin: 5px 0; font-weight: 800; transition: color 0.3s; }
.dist.near { color: #28a745; }
.label { text-transform: uppercase; font-size: 0.75rem; color: #999; letter-spacing: 1px; }

.progress-section { margin-bottom: 20px; }
.progress-bar-bg { height: 10px; background: #eee; border-radius: 5px; overflow: hidden; }
.progress-bar-fill { height: 100%; background: #28a745; transition: width 0.6s cubic-bezier(0.175, 0.885, 0.32, 1.275); }
.steps-text { font-size: 0.8rem; color: #888; text-align: right; margin-top: 5px; }

.checkpoints-log { flex-grow: 1; overflow-y: auto; background: #fff; padding: 10px; border-radius: 8px; border: 1px solid #f0f0f0; margin-bottom: 20px; }
.checkpoints-log li { font-size: 0.85rem; padding: 6px 0; border-bottom: 1px solid #fafafa; color: #666; display: flex; align-items: center; gap: 8px; }

.map-card { flex: 2; border-radius: 15px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.1); }
#active-map { width: 100%; height: 100%; z-index: 1; }

.btn { width: 100%; padding: 14px; border-radius: 8px; font-weight: bold; transition: 0.3s; border: none; cursor: pointer; display: flex; justify-content: center; gap: 10px; align-items: center; }
.btn-abandon { background: #f8f9fa; color: #666; border: 1px solid #ddd; }
.btn-abandon:hover { background: #fff; color: #cc072a; border-color: #cc072a; }
.btn-finish { background: #28a745; color: white; }

@keyframes blink { 50% { opacity: 0; } }
@keyframes pulse { 0% { box-shadow: 0 0 0 0 rgba(204, 7, 42, 0.4); } 70% { box-shadow: 0 0 0 10px rgba(204, 7, 42, 0); } 100% { box-shadow: 0 0 0 0 rgba(204, 7, 42, 0); } }
</style>