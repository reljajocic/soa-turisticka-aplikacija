<template>
  <div class="container">
    <div v-if="loading" class="text-center">Loading tour details...</div>
    <div v-else-if="!tour" class="text-center">Tour not found.</div>

    <div v-else class="tour-page">
      
      <div class="top-section">
          
          <div class="info-card card">
            <button @click="$router.back()" class="btn-back">
                <i class="fa fa-arrow-left"></i> Back
            </button>
            
            <div class="tour-hero-image" v-if="tour.imageUrl">
                <img :src="tour.imageUrl" alt="Tour Cover" />
                
                <span class="hero-level-badge" :class="'diff-' + tour.difficulty">
                    Level {{ tour.difficulty }}
                </span>
            </div>

            <div class="header">
              <h1>{{ tour.name }}</h1>
            </div>

            <p class="price-tag" v-if="!isPurchased">${{ tour.price }}</p>
            <p class="purchased-tag" v-else><i class="fa fa-check-circle"></i> Owned</p>

            <div class="tags">
              <span v-for="tag in tour.tags" :key="tag" class="tag">#{{ tag }}</span>
            </div>

            <div class="description">
              <h3>Description</h3>
              <p>{{ tour.description }}</p>
            </div>

            <div class="action-buttons">
                <template v-if="authStore.isTourist()">
                    <button v-if="isPurchased" @click="startTour" class="btn btn-start-lg">
                       <i class="fa fa-external-link-alt"></i> Start Tour
                    </button>
                    <button v-else @click="addToCart" class="btn btn-cart-lg">
                       <i class="fa fa-shopping-cart"></i> Add to Cart
                    </button>
                </template>

                <div v-else-if="authStore.isGuide()" class="guide-msg">
                    <i class="fa fa-user-tie"></i> Author View
                </div>

                <button v-else @click="$router.push('/login')" class="btn btn-login-lg">
                   <i class="fa fa-sign-in"></i> Login to Buy
                </button>
            </div>
          </div>

          <div class="map-card card">
            <div class="map-container-relative">
                <div v-if="isLocked" class="map-overlay">
                    <div class="overlay-content">
                        <i class="fa fa-lock fa-3x"></i>
                        <h3>Route Locked</h3>
                        <p>Buy the tour to reveal the full path.</p>
                    </div>
                </div>
                <div id="map-detail" :class="{ 'blurred-map': isLocked }"></div>
            </div>
          </div>
      </div>

      <div class="itinerary-section card">
          <div class="itinerary-header">
              <h3>Itinerary</h3>
              <span class="stops-count">{{ tour.keypoints.length }} Stops</span>
          </div>
          
          <div v-if="visibleKeypoints.length > 0" class="itinerary-list">
              <div v-for="(kp, i) in visibleKeypoints" :key="i" class="itinerary-item">
                  <div class="kp-image-large">
                      <img :src="kp.imageUrl || kp.image || 'https://via.placeholder.com/300x200?text=No+Image'" alt="Location" />
                      <div class="kp-index-badge">{{ i + 1 }}</div>
                  </div>
                  <div class="kp-content-large">
                      <h4 class="kp-title">{{ kp.name }}</h4>
                      <p class="kp-desc">{{ kp.description }}</p>
                  </div>
              </div>
          </div>

          <div v-if="isLocked" class="locked-content">
              <i class="fa fa-lock"></i>
              <p>Purchase this tour to see {{ tour.keypoints.length - 1 }} more stops and photos.</p>
          </div>

          <p v-else-if="tour.keypoints.length === 0" class="no-points">No keypoints added yet.</p>
      </div>

    </div>
  </div>
</template>

<script setup>
import { onMounted, ref, computed } from 'vue'
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

const isLocked = computed(() => {
    if (!authStore.isTourist()) return false
    return !isPurchased.value
})

const visibleKeypoints = computed(() => {
    if (!tour.value?.keypoints) return []
    if (isLocked.value) {
        return tour.value.keypoints.length > 0 ? [tour.value.keypoints[0]] : []
    }
    return tour.value.keypoints
})

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
        if (authStore.isTourist()) {
            await checkPurchaseStatus()
        }
        setTimeout(() => initMap(tour.value), 100)
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
    } catch (e) { console.log("Not purchased") }
}

const initMap = (t) => {
    const pointsToDraw = isLocked.value 
        ? (t.keypoints.length > 0 ? [t.keypoints[0]] : []) 
        : t.keypoints

    const startLat = pointsToDraw?.[0]?.lat || 45.2671
    const startLon = pointsToDraw?.[0]?.lon || 19.8335
    
    if (map) { map.remove(); map = null; }
    
    map = L.map('map-detail').setView([startLat, startLon], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap'
    }).addTo(map);

    if(pointsToDraw && pointsToDraw.length > 0) {
        const latlngs = []
        pointsToDraw.forEach(kp => {
            const imgHtml = (kp.imageUrl || kp.image) 
                ? `<img src="${kp.imageUrl || kp.image}" style="width:100%; height:100px; object-fit:cover; border-radius:4px; margin-bottom:5px;">` 
                : '';
            const popupContent = `<div style="text-align:center;">${imgHtml}<b>${kp.name}</b></div>`;

            L.marker([kp.lat, kp.lon], { icon: RedIcon }) 
             .addTo(map)
             .bindPopup(popupContent); 
            
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

const startTour = async () => {
    try {
        const execId = await executionStore.startTour(tour.value.id)
        const routeData = router.resolve({ 
            name: 'active-tour', 
            params: { id: execId },
            query: { tourId: tour.value.id }
        });
        window.open(routeData.href, '_blank');
    } catch (e) {
        alert("Error starting tour")
    }
}
</script>

<style scoped>
.container { max-width: 1200px; margin: 0 auto; padding-bottom: 50px; }
.text-center { text-align: center; margin-top: 30px; font-size: 1.2rem; }

/* GLAVNI LAYOUT */
.tour-page { display: flex; flex-direction: column; gap: 30px; margin-top: 20px; }

.top-section { display: flex; gap: 30px; align-items: stretch; }
.info-card { flex: 1; padding: 30px; border-radius: 12px; border: none; box-shadow: 0 4px 15px rgba(0,0,0,0.05); }
.map-card { flex: 1.5; padding: 0; overflow: hidden; border-radius: 12px; border: none; box-shadow: 0 4px 15px rgba(0,0,0,0.05); }

@media (max-width: 900px) {
    .top-section { flex-direction: column; }
    .map-card { height: 400px; }
}

/* HERO SLIKA */
.tour-hero-image {
    width: 100%; height: 250px;
    border-radius: 8px; overflow: hidden;
    margin-bottom: 20px;
    box-shadow: 0 4px 10px rgba(0,0,0,0.1);
    position: relative; /* ZA BEDŽ */
}
.tour-hero-image img { width: 100%; height: 100%; object-fit: cover; }

/* BEDŽ PREKO SLIKE */
.hero-level-badge {
    position: absolute; top: 15px; right: 15px;
    color: white; padding: 6px 12px; border-radius: 20px;
    font-weight: bold; font-size: 0.9rem; text-transform: uppercase;
    box-shadow: 0 2px 5px rgba(0,0,0,0.3);
}

/* BOJE (Iste kao ToursView) */
.diff-1 { background: #2ecc71; } 
.diff-2 { background: #f1c40f; color: #333; } 
.diff-3 { background: #e67e22; } 
.diff-4 { background: #e74c3c; } 
.diff-5 { background: #8e44ad; } 

.header h1 { font-size: 1.8rem; margin: 0; line-height: 1.2; color: #333; }

.price-tag { font-size: 2rem; font-weight: 800; color: #2c3e50; margin: 10px 0; }
.purchased-tag { font-size: 1.5rem; font-weight: 700; color: #27ae60; margin: 10px 0; display: flex; align-items: center; gap: 8px; }

.tags { margin-bottom: 20px; }
.tag { background: #f0f2f5; color: #cc072a; padding: 4px 10px; border-radius: 15px; margin-right: 5px; font-size: 0.85rem; font-weight: 600; }
.description { border-top: 1px solid #eee; padding-top: 20px; color: #555; line-height: 1.6; margin-bottom: 30px; }

.btn-back { background: none; border: none; color: #666; cursor: pointer; margin-bottom: 15px; font-size: 1rem; padding: 0; display: flex; align-items: center; gap: 5px; }
.btn-back:hover { color: #cc072a; }

.action-buttons button { width: 100%; padding: 15px; border-radius: 8px; font-size: 1.1rem; font-weight: bold; border: none; cursor: pointer; display: flex; justify-content: center; gap: 10px; transition: 0.2s; }
.btn-cart-lg { background: #cc072a; color: white; }
.btn-start-lg { background: #2ecc71; color: white; }
.btn-login-lg { background: #34495e; color: white; }
.guide-msg { padding: 15px; background: #f8f9fa; border-left: 4px solid #cc072a; }

.itinerary-section { padding: 40px; background: white; border-radius: 12px; box-shadow: 0 4px 15px rgba(0,0,0,0.05); }
.itinerary-header { display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid #f0f0f0; padding-bottom: 20px; margin-bottom: 30px; }
.itinerary-header h3 { margin: 0; font-size: 1.8rem; color: #2c3e50; }
.stops-count { background: #f0f2f5; color: #555; padding: 5px 15px; border-radius: 20px; font-weight: 600; }

.itinerary-list { display: flex; flex-direction: column; gap: 30px; }
.itinerary-item { display: flex; gap: 30px; align-items: flex-start; padding-bottom: 30px; border-bottom: 1px solid #eee; }
.itinerary-item:last-child { border-bottom: none; padding-bottom: 0; }

.kp-image-large { position: relative; width: 280px; height: 180px; flex-shrink: 0; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.1); }
.kp-image-large img { width: 100%; height: 100%; object-fit: cover; transition: transform 0.3s; }
.kp-image-large:hover img { transform: scale(1.05); }

.kp-index-badge { position: absolute; top: 10px; left: 10px; background: #cc072a; color: white; width: 35px; height: 35px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: 800; font-size: 1.1rem; box-shadow: 0 2px 5px rgba(0,0,0,0.3); }

.kp-content-large { flex: 1; }
.kp-title { margin: 0 0 10px 0; font-size: 1.4rem; color: #2c3e50; }
.kp-desc { color: #555; line-height: 1.7; font-size: 1.05rem; white-space: pre-line; }

@media (max-width: 600px) {
    .itinerary-item { flex-direction: column; }
    .kp-image-large { width: 100%; height: 200px; }
}

.map-container-relative { position: relative; height: 100%; min-height: 500px; width: 100%; }
#map-detail { height: 100%; width: 100%; z-index: 1; }
.blurred-map { filter: blur(5px); pointer-events: none; }
.map-overlay { position: absolute; top: 0; left: 0; width: 100%; height: 100%; z-index: 10; display: flex; justify-content: center; align-items: center; background: rgba(255, 255, 255, 0.5); }
.overlay-content { background: white; padding: 20px 40px; border-radius: 12px; text-align: center; box-shadow: 0 5px 20px rgba(0,0,0,0.2); }
.locked-content { background: #fdf2f2; border: 1px dashed #cc072a; padding: 20px; text-align: center; color: #cc072a; font-weight: bold; margin-top: 20px; border-radius: 8px; }
</style>