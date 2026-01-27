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

        <button @click="addToCart" class="btn btn-cart-lg">
           <i class="fa fa-shopping-cart"></i> Add to Cart
        </button>
      </div>

      <div class="map-section card">
        <div id="map-detail" style="height: 600px; width: 100%; z-index: 1;"></div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { useTourStore } from '@/stores/tours'
import { useCartStore } from '@/stores/cart'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css' 
// Ovi importi nam trebaju samo kao fallback, ali koristićemo RedIcon
import icon from 'leaflet/dist/images/marker-icon.png'
import iconShadow from 'leaflet/dist/images/marker-shadow.png'

const route = useRoute()
const tourStore = useTourStore()
const cartStore = useCartStore()

const tour = ref(null)
const loading = ref(true)
let map = null

// --- DEFINICIJA CRVENOG MARKERA ---
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
    }
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
})

const initMap = (t) => {
    const startLat = t.keypoints?.[0]?.lat || 45.2671
    const startLon = t.keypoints?.[0]?.lon || 19.8335
    
    map = L.map('map-detail').setView([startLat, startLon], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(map);

    if(t.keypoints && t.keypoints.length > 0) {
        const latlngs = []
        
        t.keypoints.forEach(kp => {
            // OVDE KORISTIMO CRVENI MARKER
            L.marker([kp.lat, kp.lon], { icon: RedIcon }) 
             .addTo(map)
             .bindPopup(`<b>${kp.name}</b>`); 
             
            latlngs.push([kp.lat, kp.lon])
        })
        
        if(latlngs.length > 1) {
            // Linija ostaje crvena
            L.polyline(latlngs, {color: '#cc072a', weight: 4}).addTo(map);
            map.fitBounds(latlngs);
        }
    }
}

const addToCart = async () => {
    if(tour.value) {
        await cartStore.addToCart(tour.value)
    }
}
</script>

<style scoped>
.text-center { text-align: center; margin-top: 30px; font-size: 1.2rem; }
.tour-details-wrapper { display: flex; gap: 20px; flex-wrap: wrap; margin-top: 20px; align-items: flex-start; }

.info-section { flex: 1; min-width: 350px; padding: 30px; border-radius: 12px; box-shadow: 0 4px 15px rgba(0,0,0,0.05); border: none; }
.map-section { flex: 1.5; min-width: 400px; padding: 0; overflow: hidden; border-radius: 12px; box-shadow: 0 4px 15px rgba(0,0,0,0.05); border: none;}

/* HEADER FIX */
.header { 
    display: flex; 
    align-items: flex-start; 
    justify-content: space-between; 
    gap: 20px; 
    margin-bottom: 10px; 
}
.header h1 { margin: 0; font-size: 2rem; color: #2c3e50; line-height: 1.2; }

/* BADGES */
.badge { 
    padding: 6px 12px; 
    border-radius: 8px; 
    color: white; 
    font-weight: bold; 
    font-size: 0.9rem;
    white-space: nowrap; 
    flex-shrink: 0; 
}
.diff-1 { background: #cc072a; } 
.diff-2 { background: #b00626; } 
.diff-3 { background: #99051f; } 
.diff-4 { background: #7a0419; } 
.diff-5 { background: #4d020f; }

.price-tag { font-size: 2rem; font-weight: 800; color: #2c3e50; margin: 5px 0 15px 0; }

.tags { margin-bottom: 25px; }
.tag { background: #f0f2f5; color: #cc072a; padding: 5px 10px; border-radius: 20px; margin-right: 8px; font-size: 0.85rem; font-weight: 500;}

.description { line-height: 1.6; color: #555; margin-bottom: 30px; border-top: 1px solid #eee; padding-top: 20px; }
.description h3 { margin-top: 0; color: #333; }

.btn-back { background: none; border: none; color: #666; cursor: pointer; margin-bottom: 20px; font-size: 1rem; padding: 0; display: flex; align-items: center; gap: 5px;}
.btn-back:hover { color: #cc072a; }

/* DUGME */
.btn-cart-lg { 
    width: 100%; 
    padding: 15px; 
    background: #cc072a; 
    color: white; 
    border: none; 
    border-radius: 8px; 
    font-size: 1.2rem; 
    cursor: pointer; 
    font-weight: bold; 
    margin-top: 30px; 
    transition: 0.2s; 
    display: flex; 
    align-items: center; 
    justify-content: center; 
    gap: 10px; 
    box-shadow: 0 4px 6px rgba(0,0,0,0.1);
}
.btn-cart-lg:hover { 
    background: #99051f; 
    transform: scale(1.02); 
}

/* ITINERARY LISTA */
.keypoints-list h3 { border-bottom: 1px solid #eee; padding-bottom: 10px; margin-bottom: 15px; }
.itinerary-list { list-style: none; padding: 0; }
.itinerary-list li { 
    display: flex; 
    align-items: center; 
    gap: 15px; 
    margin-bottom: 15px; 
    padding: 10px;
    background: #f9f9f9;
    border-radius: 8px;
}
.kp-index {
    background: #cc072a;
    color: white;
    width: 25px;
    height: 25px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: bold;
    font-size: 0.9rem;
}
.kp-name { font-weight: 600; color: #333; font-size: 1.05rem; }
</style>