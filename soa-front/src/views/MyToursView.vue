<template>
  <div class="container">
    
    <div class="header-section">
      <h2>My Purchased Tours</h2>
      <button @click="$router.push('/tours')" class="btn btn-primary">
        Explore More Tours
      </button>
    </div>

    <div v-if="loading" class="text-center">Loading your tours...</div>
    
    <div v-else-if="tours.length === 0" class="empty-state">
      <i class="fa fa-ticket-alt empty-icon"></i>
      <h3>No tours purchased yet.</h3>
      <p>Visit the <router-link to="/tours">Tours page</router-link> to find your next adventure!</p>
    </div>

    <div v-else class="tours-grid">
      <div v-for="tour in tours" :key="tour.id" class="tour-card">
        
        <div class="card-header" :class="'diff-' + tour.difficulty">
          <span class="difficulty-badge">Level {{ tour.difficulty }}</span>
        </div>

        <div class="card-body">
          <h3>{{ tour.name }}</h3>
          
          <p class="tags">
            <span v-for="tag in tour.tags" :key="tag" class="tag">#{{ tag }}</span>
          </p>
          
          <p class="description">{{ truncateText(tour.description, 80) }}</p>
          
          <div class="card-footer">
            <span class="price">${{ tour.price }}</span>
            
            <div class="buttons">
                <button @click="$router.push(`/tour/${tour.id}`)" class="btn btn-details" title="View Details">
                  <i class="fa fa-info-circle" aria-hidden="true"></i> 
                </button>

                <button @click="startTour(tour)" class="btn btn-start" title="Start Adventure">
                   <i class="fa fa-play"></i> Start
                </button>
            </div>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useTourStore } from '@/stores/tours'
import { useExecutionStore } from '@/stores/execution'

const router = useRouter()
const tourStore = useTourStore()
const executionStore = useExecutionStore()

const tours = ref([])
const loading = ref(true)

onMounted(async () => {
  try {
    // Dovlačimo samo kupljene ture
    tours.value = await tourStore.getPurchasedTours()
  } catch (err) {
    console.error("Error fetching purchased tours", err)
  } finally {
    loading.value = false
  }
})

// --- GLAVNA PROMENA: START U NOVOM TABU ---
const startTour = async (tour) => {
    try {
        // 1. Kreiraj sesiju na serveru
        const execId = await executionStore.startTour(tour.id)
        
        // 2. Generiši URL za Active Tour stranicu
        const routeData = router.resolve({ 
            name: 'active-tour', 
            params: { id: execId },
            query: { tourId: tour.id }
        });
        
        // 3. Otvori u novom tabu
        window.open(routeData.href, '_blank');

    } catch (e) {
        alert("Cannot start tour: " + (e.response?.data?.message || "Error starting tour"))
    }
}

// Helper za skraćivanje teksta
const truncateText = (text, length) => {
  if (!text) return ''
  return text.length > length ? text.substring(0, length) + '...' : text
}
</script>

<style scoped>
/* HEADER */
.header-section {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin: 30px 0;
    border-bottom: 1px solid #eee;
    padding-bottom: 20px;
}
.header-section h2 { margin: 0; color: #2c3e50; }

/* GRID */
.tours-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 25px; }

/* KARTICA (Isti stil kao ToursView) */
.tour-card { background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.1); transition: transform 0.2s; display: flex; flex-direction: column; }
.tour-card:hover { box-shadow: 0 4px 15px rgba(0,0,0,0.25); transform: translateY(-5px); transition-duration: 0.3s; }

.card-header { height: 80px; position: relative; background: #ddd; }
/* Boje za težinu */
.diff-1 { background: #cc072a; } .diff-2 { background: #b00626; } .diff-3 { background: #99051f; } .diff-4 { background: #7a0419; } .diff-5 { background: #4d020f; } 

.difficulty-badge { position: absolute; top: 10px; right: 10px; background: rgba(0,0,0,0.6); color: white; padding: 4px 8px; border-radius: 12px; font-size: 0.8rem; font-weight: bold; }

.card-body { padding: 20px; display: flex; flex-direction: column; flex-grow: 1; }
.card-body h3 { font-size: 1.2rem; margin-top: 0; margin-bottom: 5px; }

.tags { font-size: 0.8rem; color: #cc072a; margin-bottom: 10px; }
.tag { margin-right: 5px; }

.description { color: #666; font-size: 0.85rem; margin-bottom: 20px; flex-grow: 1; }

/* FOOTER KARTICE */
.card-footer { display: flex; justify-content: space-between; align-items: center; font-weight: bold; border-top: 1px solid #eee; padding-top: 15px; margin-top: auto; }
.price { color: #2c3e50; font-size: 1.3rem; }

.buttons { display: flex; gap: 10px; }

/* DUGMIĆI */
.btn-details { 
    background: #cc072a; color: white; border: none;
    width: 35px; height: 35px; border-radius: 50%; 
    display: flex; align-items: center; justify-content: center;
    cursor: pointer; transition: 0.2s;
}
.btn-details:hover { background: #99051f; transform: scale(1.1); }

.btn-start { 
    background: #2ecc71; color: white; border: none; 
    padding: 8px 16px; border-radius: 20px; 
    cursor: pointer; transition: 0.2s; 
    font-weight: bold; font-size: 0.9rem;
    display: flex; align-items: center; gap: 5px;
}
.btn-start:hover { background: #27ae60; transform: scale(1.05); }

/* EMPTY STATE */
.empty-state { text-align: center; margin-top: 50px; color: #777; }
.empty-icon { font-size: 3rem; color: #ccc; margin-bottom: 20px; }
.empty-state a { color: #cc072a; font-weight: bold; text-decoration: none; }
</style>