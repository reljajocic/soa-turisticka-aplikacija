<template>
  <div class="container">
    <div v-if="loading" class="text-center">Loading tour details...</div>
    <div v-else-if="!tour" class="text-center">Tour not found.</div>

    <div v-else class="tour-details-wrapper">
      
      <div class="info-section card">
        <button @click="$router.back()" class="btn-back">
            <i class="fa fa-arrow-left"></i> Back
        </button>
        
        <div class="header">
          <h1>{{ tour.name }}</h1>
          <span class="badge" :class="'diff-' + tour.difficulty">Level {{ tour.difficulty }}</span>
        </div>

        <p class="price-tag">${{ tour.price }}</p>

        <div class="tags">
          <span v-for="tag in tour.tags" :key="tag" class="tag">#{{ tag }}</span>
        </div>

        <div class="description">
          <h3>Description</h3>
          <p>{{ tour.description }}</p>
        </div>

        <div class="keypoints-list" v-if="tour.keypoints && tour.keypoints.length">
            <h3>Itinerary ({{ tour.keypoints.length }} stops)</h3>
            <ul class="itinerary-list">
                <li v-for="(kp, i) in tour.keypoints" :key="i">
                    <span class="kp-index">{{ i + 1 }}</span>
                    <span class="kp-name">{{ kp.name }}</span>
                </li>
            </ul>
        </div>
        <div v-else class="keypoints-list">
            <p>No keypoints added for this tour yet.</p>
        </div>

        <div class="action-buttons">
            
            <template v-if="authStore.isTourist()">
                <button v-if="isPurchased" @click="startTour" class="btn btn-start-lg">
                   <i class="fa fa-external-link-alt"></i> Start Tour (New Tab)
                </button>
                <button v-else @click="addToCart" class="btn btn-cart-lg">
                   <i class="fa fa-shopping-cart"></i> Add to Cart
                </button>
            </template>

            <div v-else-if="authStore.isGuide()" class="guide-msg">
                <i class="fa fa-user-tie"></i> You are viewing this tour as an Author/Guide.
            </div>

            <button v-else @click="$router.push('/login')" class="btn btn-login-lg">
               <i class="fa fa-sign-in"></i> Login to Buy
            </button>

        </div>

      </div>

      <div class="map-section card">
        <div id="map-detail" style="height: 600px; width: 100%; z-index: 1;"></div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useTourStore } from '@/stores/tours'
import { useCartStore } from '@/stores/cart'
import { useExecutionStore } from '@/stores/execution'
import { useAuthStore } from '@/stores/auth'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css' 

const route = useRoute()
const router = useRouter()
const tourStore = useTourStore()
const cartStore = useCartStore()
const executionStore = useExecutionStore()
const authStore = useAuthStore()

const tour = ref(null)
const loading = ref(true)
const isPurchased = ref(false)
let map = null

const RedIcon = new L.Icon({
  iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41]
});

onMounted(async () => {
  try {
    const tourId = route.params.id
    tour.value = await tourStore.getTour(tourId)
    
    if (tour.value) {
        setTimeout(() => initMap(tour.value), 100)
        if (authStore.isTourist()) {
            await checkPurchaseStatus()
        }
    }
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
})

const checkPurchaseStatus = async () => {
    try {
        const myTours = await tourStore.getPurchasedTours()
        if (myTours && myTours.length > 0) {
            isPurchased.value = myTours.some(t => t.id === tour.value.id)
        }
    } catch (e) {
        console.log("Not purchased or not logged in")
    }
}

const initMap = (t) => {
    const startLat = t.keypoints?.[0]?.lat || 45.2671
    const startLon = t.keypoints?.[0]?.lon || 19.8335
    
    map = L.map('map-detail').setView([startLat, startLon], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap'
    }).addTo(map);

    if(t.keypoints && t.keypoints.length > 0) {
        const latlngs = []
        t.keypoints.forEach(kp => {
            L.marker([kp.lat, kp.lon], { icon: RedIcon }) 
             .addTo(map)
             .bindPopup(`<b>${kp.name}</b>`); 
            latlngs.push([kp.lat, kp.lon])
        })
        if(latlngs.length > 1) {
            L.polyline(latlngs, {color: '#cc072a', weight: 4}).addTo(map);
            map.fitBounds(latlngs);
        }
    }
}

const addToCart = async () => {
    if(tour.value) await cartStore.addToCart(tour.value)
}

// --- IZMENJENA FUNKCIJA ZA NOVI TAB ---
const startTour = async () => {
    try {
        // 1. Kreiramo sesiju na backendu
        const execId = await executionStore.startTour(tour.value.id)
        
        // 2. Umesto router.push, generišemo URL
        const routeData = router.resolve({ 
            name: 'active-tour', 
            params: { id: execId },
            query: { tourId: tour.value.id }
        });
        
        // 3. Otvaramo taj URL u novom tabu ('_blank')
        window.open(routeData.href, '_blank');
        
    } catch (e) {
        alert("Cannot start tour: " + (e.response?.data?.message || "Error starting tour"))
    }
}
</script>

<style scoped>
.text-center { text-align: center; margin-top: 30px; font-size: 1.2rem; }
.tour-details-wrapper { display: flex; gap: 20px; flex-wrap: wrap; margin-top: 20px; align-items: flex-start; }

.info-section { flex: 1; min-width: 350px; padding: 30px; border-radius: 12px; box-shadow: 0 4px 15px rgba(0,0,0,0.05); border: none; }
.map-section { flex: 1.5; min-width: 400px; padding: 0; overflow: hidden; border-radius: 12px; box-shadow: 0 4px 15px rgba(0,0,0,0.05); border: none;}

.header { display: flex; align-items: flex-start; justify-content: space-between; gap: 20px; margin-bottom: 10px; }
.header h1 { margin: 0; font-size: 2rem; color: #2c3e50; line-height: 1.2; }

/* BADGES */
.badge { padding: 6px 12px; border-radius: 8px; color: white; font-weight: bold; font-size: 0.9rem; white-space: nowrap; flex-shrink: 0; }
.diff-1 { background: #cc072a; } .diff-2 { background: #b00626; } .diff-3 { background: #99051f; } .diff-4 { background: #7a0419; } .diff-5 { background: #4d020f; }

.price-tag { font-size: 2rem; font-weight: 800; color: #2c3e50; margin: 5px 0 15px 0; }
.tags { margin-bottom: 25px; }
.tag { background: #f0f2f5; color: #cc072a; padding: 5px 10px; border-radius: 20px; margin-right: 8px; font-size: 0.85rem; font-weight: 500;}

.description { line-height: 1.6; color: #555; margin-bottom: 30px; border-top: 1px solid #eee; padding-top: 20px; }
.description h3 { margin-top: 0; color: #333; }

.btn-back { background: none; border: none; color: #666; cursor: pointer; margin-bottom: 20px; font-size: 1rem; padding: 0; display: flex; align-items: center; gap: 5px;}
.btn-back:hover { color: #cc072a; }

/* BUTTONS */
.action-buttons { margin-top: 30px; }

.btn-cart-lg, .btn-start-lg, .btn-login-lg { 
    width: 100%; padding: 15px; border: none; border-radius: 8px; font-size: 1.2rem; 
    cursor: pointer; font-weight: bold; transition: 0.2s; 
    display: flex; align-items: center; justify-content: center; gap: 10px; 
    box-shadow: 0 4px 6px rgba(0,0,0,0.1);
}

.btn-cart-lg { background: #cc072a; color: white; }
.btn-cart-lg:hover { background: #99051f; transform: scale(1.02); }

.btn-start-lg { background: #2ecc71; color: white; }
.btn-start-lg:hover { background: #27ae60; transform: scale(1.02); }

.btn-login-lg { background: #34495e; color: white; }
.btn-login-lg:hover { background: #2c3e50; transform: scale(1.02); }

.guide-msg {
    padding: 15px; background-color: #f8f9fa; border-left: 4px solid #cc072a;
    color: #555; font-weight: 500; display: flex; align-items: center; gap: 10px;
}

/* ITINERARY */
.keypoints-list h3 { border-bottom: 1px solid #eee; padding-bottom: 10px; margin-bottom: 15px; }
.itinerary-list { list-style: none; padding: 0; }
.itinerary-list li { display: flex; align-items: center; gap: 15px; margin-bottom: 15px; padding: 10px; background: #f9f9f9; border-radius: 8px; }
.kp-index { background: #cc072a; color: white; width: 25px; height: 25px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: bold; font-size: 0.9rem; }
.kp-name { font-weight: 600; color: #333; font-size: 1.05rem; }
</style>