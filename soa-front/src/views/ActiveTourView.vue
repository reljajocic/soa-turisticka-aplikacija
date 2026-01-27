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
        
        <div class="progress-section">
            <div class="step-info">
                <span>Current Goal:</span>
                <strong>{{ currentTargetPoint?.name || 'Finish' }}</strong>
            </div>
            
            <div class="progress-bar-bg">
                <div class="progress-bar-fill" :style="{ width: progressPercent + '%' }"></div>
            </div>
            
            <p class="steps-text">{{ progressIndex }} / {{ tour?.keypoints?.length }} checkpoints visited</p>
        </div>

        <div class="info-box">
            <div v-if="isFinished" class="finished-msg">
                <i class="fa fa-trophy"></i> 
                <div>
                    <h4>CONGRATULATIONS!</h4>
                    <p>You have completed the tour.</p>
                </div>
            </div>
            
            <div v-else>
                <p class="label">Distance to next stop:</p>
                <h1 class="dist" :class="{'near': distanceKm < 0.1}">{{ distanceDisplay }}</h1>
                <small class="hint">
                    <i class="fa fa-info-circle"></i> 
                    Open <b>Simulator</b> in another tab and click near the point to move.
                </small>
            </div>
        </div>

        <div class="actions">
            <button v-if="!isFinished" @click="abandon" class="btn btn-abandon">
                <i class="fa fa-times"></i> Abandon Tour
            </button>
            <button v-else @click="$router.push('/my-tours')" class="btn btn-finish">
                Back to My Tours
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
import icon from 'leaflet/dist/images/marker-icon.png'
import iconShadow from 'leaflet/dist/images/marker-shadow.png'

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
let pollingInterval = null
let map = null
let markers = []
let routeLine = null

// --- COMPUTED ---
const currentTargetPoint = computed(() => {
    if(!tour.value || !tour.value.keypoints) return null
    if(progressIndex.value >= tour.value.keypoints.length) return null
    return tour.value.keypoints[progressIndex.value]
})

const progressPercent = computed(() => {
    if(!tour.value || !tour.value.keypoints) return 0
    return (progressIndex.value / tour.value.keypoints.length) * 100
})

const distanceDisplay = computed(() => {
    if(distanceKm.value === null) return "Calculating..."
    if(distanceKm.value < 1) return (distanceKm.value * 1000).toFixed(0) + " m"
    return distanceKm.value.toFixed(2) + " km"
})

// --- LEAFLET ICONS ---
const DefaultIcon = L.icon({
    iconUrl: icon, shadowUrl: iconShadow,
    iconSize: [25, 41], iconAnchor: [12, 41], popupAnchor: [1, -34]
});
const GreenIcon = new L.Icon({
  iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-green.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
  iconSize: [25, 41], iconAnchor: [12, 41], popupAnchor: [1, -34], shadowSize: [41, 41]
});
const GreyIcon = new L.Icon({
  iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-grey.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
  iconSize: [25, 41], iconAnchor: [12, 41], popupAnchor: [1, -34], shadowSize: [41, 41]
});

// --- LIFECYCLE ---
onMounted(async () => {
    try {
        // 1. Učitaj podatke o turi da možemo da nacrtamo mapu
        tour.value = await tourStore.getTour(tourId)
        
        // 2. Proveri status izvršavanja odmah
        await pollBackend()
        
        loading.value = false
        
        // 3. Inicijalizuj mapu (malo kašnjenje da se DOM učita)
        setTimeout(initMap, 100)

        // 4. Pokreni timer koji pita backend svakih 3 sekunde
        pollingInterval = setInterval(pollBackend, 3000)

    } catch (e) {
        alert("Error loading tour data")
        router.push('/my-tours')
    }
})

onUnmounted(() => {
    if(pollingInterval) clearInterval(pollingInterval)
})

// --- MAP LOGIC ---
const initMap = () => {
    if(!tour.value || !tour.value.keypoints.length) return
    const startKp = tour.value.keypoints[0]
    map = L.map('active-map').setView([startKp.lat, startKp.lon], 14)

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap'
    }).addTo(map)

    const latlngs = []
    
    // Dodaj markere
    tour.value.keypoints.forEach((kp, i) => {
        let icon = DefaultIcon
        if (i < progressIndex.value) icon = GreenIcon // Posećeno
        
        const m = L.marker([kp.lat, kp.lon], { icon: icon })
            .addTo(map)
            .bindPopup(`<b>${i+1}. ${kp.name}</b>`)
        
        markers.push(m)
        latlngs.push([kp.lat, kp.lon])
    })

    // Nacrtaj liniju
    if(latlngs.length > 1) {
        routeLine = L.polyline(latlngs, {color: '#cc072a', weight: 4, opacity: 0.7}).addTo(map);
        map.fitBounds(latlngs)
    }
}

const updateMapMarkers = () => {
    if(!map) return
    markers.forEach((m, i) => {
        if (i < progressIndex.value) {
            m.setIcon(GreenIcon) // Zeleno ako je posećeno
        } else if (i === progressIndex.value) {
            m.setIcon(DefaultIcon) // Plavo (Standard) ako je trenutni cilj
            m.openPopup()
        } else {
            m.setIcon(GreyIcon) // Sivo ako tek sledi
        }
    })
}

// --- POLLING LOGIC ---
const pollBackend = async () => {
    if(isFinished.value) return

    try {
        const res = await execStore.pollPosition(executionId)
        
        if (res.error) {
            console.error(res.error)
            return
        }

        distanceKm.value = res.distanceKm
        
        // Ako je backend rekao da smo prešli na sledeću tačku
        if (res.progress > progressIndex.value) {
            progressIndex.value = res.progress
            updateMapMarkers() // Osveži boje markera
            
            // Provera da li je kraj
            if (progressIndex.value >= tour.value.keypoints.length) {
                finishSuccess()
            }
        }

    } catch (e) {
        console.error("Polling error", e)
    }
}

const finishSuccess = async () => {
    isFinished.value = true
    clearInterval(pollingInterval)
    // Pozivamo finish na backendu
    await execStore.finishTour(executionId, true)
}

const abandon = async () => {
    if(!confirm("Are you sure you want to give up?")) return
    clearInterval(pollingInterval)
    await execStore.finishTour(executionId, false) // false = abandoned
    router.push('/my-tours')
}
</script>

<style scoped>
.container { padding-bottom: 50px; }
.loading-screen { text-align: center; margin-top: 100px; color: #cc072a; }

.active-tour-wrapper { display: flex; gap: 25px; flex-wrap: wrap; margin-top: 20px; align-items: stretch; height: calc(100vh - 150px); min-height: 600px; }

/* LEVA KARTICA */
.status-card { 
    flex: 1; min-width: 350px; padding: 30px; 
    display: flex; flex-direction: column; 
    background: white; border-radius: 12px; box-shadow: 0 4px 20px rgba(0,0,0,0.08); 
}

.header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px; }
.header h2 { margin: 0; font-size: 1.4rem; color: #333; display: flex; align-items: center; gap: 10px; }
.live-badge { background: #cc072a; color: white; padding: 4px 8px; border-radius: 4px; font-size: 0.7rem; font-weight: bold; animation: pulse 2s infinite; }
.pulse { color: #cc072a; animation: blink 1s infinite; }

@keyframes blink { 50% { opacity: 0; } }
@keyframes pulse { 0% { opacity: 1; } 50% { opacity: 0.7; } 100% { opacity: 1; } }

h3 { margin: 0 0 25px 0; color: #555; font-weight: normal; }

/* PROGRESS */
.progress-section { margin-bottom: 30px; background: #f9f9f9; padding: 15px; border-radius: 8px; }
.step-info { display: flex; justify-content: space-between; margin-bottom: 10px; font-size: 0.95rem; }
.progress-bar-bg { height: 12px; background: #e0e0e0; border-radius: 6px; overflow: hidden; }
.progress-bar-fill { height: 100%; background: #28a745; transition: width 0.5s ease-out; }
.steps-text { text-align: right; font-size: 0.8rem; color: #888; margin-top: 5px; }

/* INFO BOX */
.info-box { 
    flex-grow: 1; display: flex; flex-direction: column; justify-content: center; align-items: center;
    text-align: center; border: 2px dashed #ddd; border-radius: 12px; padding: 20px; margin-bottom: 20px;
}

.dist { font-size: 3.5rem; color: #2c3e50; margin: 10px 0; font-weight: 800; transition: color 0.3s; }
.dist.near { color: #28a745; } /* Zeleno kad je blizu */
.label { text-transform: uppercase; font-size: 0.8rem; letter-spacing: 1px; color: #999; }
.hint { display: block; margin-top: 15px; color: #cc072a; background: #fff0f0; padding: 8px 12px; border-radius: 20px; font-size: 0.85rem;}

.finished-msg { color: #28a745; display: flex; flex-direction: column; align-items: center; gap: 10px; }
.finished-msg i { font-size: 4rem; margin-bottom: 10px; }
.finished-msg h4 { margin: 0; font-size: 1.5rem; }

/* DUGMIĆI */
.actions { margin-top: auto; }
.btn { width: 100%; padding: 15px; border: none; border-radius: 8px; cursor: pointer; font-weight: bold; font-size: 1rem; display: flex; justify-content: center; gap: 10px; align-items: center; transition: 0.2s; }
.btn-abandon { background: #f8f9fa; color: #333; border: 1px solid #ddd; }
.btn-abandon:hover { background: #e2e6ea; color: #cc072a; border-color: #cc072a; }
.btn-finish { background: #28a745; color: white; }
.btn-finish:hover { background: #218838; transform: translateY(-2px); }

/* DESNA KARTICA */
.map-card { flex: 2; padding: 0; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.08); height: 100%; min-height: 500px; }
#active-map { width: 100%; height: 100%; }
</style>