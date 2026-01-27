<template>
  <div class="container">

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
            
            <div class="buttons">
                <button @click="$router.push(`/tour/${tour.id}`)" class="btn btn-details" title="View Details">
                  <i class="fa fa-info-circle btn-icon" aria-hidden="true"></i> 
                </button>

                <button @click="addToCart(tour)" class="btn btn-details" title="Add to Cart">
                   <i class="fa fa-shopping-cart" aria-hidden="true"></i>
                </button>
            </div>
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
import { useCartStore } from '@/stores/cart'

const tourStore = useTourStore()
const cartStore = useCartStore()
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

const addToCart = async (tour) => {
    await cartStore.addToCart(tour)
}

const truncateText = (text, length) => {
  if (!text) return ''
  return text.length > length ? text.substring(0, length) + '...' : text
}
</script>

<style scoped>
.page-title { text-align: center; margin: 30px 0; color: #000000; }
.tours-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 25px; }
.tour-card { background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.1); transition: transform 0.2s; display: flex; flex-direction: column; }
.tour-card:hover { box-shadow: 0 4px 15px rgba(0,0,0,0.25); transition-duration: 0.5s ;}

.card-header { height: 100px; position: relative; background: #ddd; }
.diff-1 { background: #cc072a; } 
.diff-2 { background: #b00626; } 
.diff-3 { background: #99051f; } 
.diff-4 { background: #7a0419; } 
.diff-5 { background: #4d020f; } 

.difficulty-badge { position: absolute; top: 10px; right: 10px; background: rgba(0,0,0,0.6); color: white; padding: 4px 8px; border-radius: 12px; font-size: 0.8rem; font-weight: bold; }
.card-body { padding: 20px; display: flex; flex-direction: column; flex-grow: 1; }
.card-body h3 { font-size: 1.2rem; }
.tags { font-size: 0.8rem; color: #cc072a; margin-bottom: 5px; }
.tag { margin-right: 5px; }
.description { color: #666; font-size: 0.8rem; margin-bottom: 15px; min-height: 40px; flex-grow: 1; }

.card-footer { display: flex; justify-content: space-between; align-items: center; font-weight: bold; border-top: 1px solid #eee; padding-top: 15px; margin-top: auto; }
.price { color: #2c3e50; font-size: 1.2rem; }

.buttons { display: flex; gap: 8px; }

.btn { display: flex; align-items: center; gap: 5px; justify-content: center; font-family: inherit; }

.btn-details { 
    background: #cc072a; 
    color: white;
    padding: 8px 12px; border-radius: 6px; cursor: pointer; transition: 0.2s; font-weight: 500;
}
.btn-details:hover { background: #99051f; color: white; transform: scale(1.05);}

.btn-cart { 
    background: #ff9800; color: white; border: none; 
    padding: 8px 12px; border-radius: 6px; cursor: pointer; transition: 0.2s; 
    font-size: 1.1rem; /* Malo veća ikonica za korpu */
}
.btn-cart:hover { background: #e65100; transform: scale(1.05); }

.btn-icon{
    color: white;
}
</style>