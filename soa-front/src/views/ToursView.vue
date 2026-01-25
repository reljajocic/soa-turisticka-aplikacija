<template>
  <div class="container">
    <h2 class="page-title">Explore Tours</h2>

    <div v-if="loading" class="text-center">Loading tours...</div>
    
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
            <span class="price">{{ tour.price }} $</span>
            <button class="btn btn-outline">View Details</button>
          </div>
        </div>
      </div>
    </div>
    
    <div v-if="!loading && tours.length === 0" class="empty-state">
      <p>No tours found yet.</p>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { useTourStore } from '@/stores/tours'

const tourStore = useTourStore()
const tours = ref([])
const loading = ref(true)

onMounted(async () => {
  try {
    tours.value = await tourStore.getAllTours()
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
})

const truncateText = (text, length) => {
  if (!text) return ''
  return text.length > length ? text.substring(0, length) + '...' : text
}
</script>

<style scoped>
.page-title { text-align: center; margin: 30px 0; color: #2c3e50; }
.tours-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 25px; }
.tour-card { background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.1); transition: transform 0.2s; }
.tour-card:hover { transform: translateY(-5px); }

.card-header { height: 100px; position: relative; background: #ddd; }
.diff-1 { background: #8bc34a; } /* Lako - Zeleno */
.diff-2 { background: #ffeb3b; } 
.diff-3 { background: #ff9800; } 
.diff-4 { background: #f44336; } 
.diff-5 { background: #b71c1c; } /* Teško - Crveno */

.difficulty-badge { position: absolute; top: 10px; right: 10px; background: rgba(0,0,0,0.6); color: white; padding: 4px 8px; border-radius: 12px; font-size: 0.8rem; font-weight: bold; }
.card-body { padding: 20px; }
.card-body h3 { margin: 0 0 10px; font-size: 1.2rem; }
.tags { font-size: 0.8rem; color: #007bff; margin-bottom: 10px; }
.tag { margin-right: 5px; }
.description { color: #666; font-size: 0.9rem; margin-bottom: 15px; min-height: 40px;}
.card-footer { display: flex; justify-content: space-between; align-items: center; font-weight: bold; border-top: 1px solid #eee; padding-top: 10px; }
.price { color: #2c3e50; font-size: 1.2rem; }
.btn-outline { background: white; border: 1px solid #007bff; color: #007bff; padding: 5px 15px; border-radius: 6px; cursor: pointer; }
.btn-outline:hover { background: #007bff; color: white; }
</style>