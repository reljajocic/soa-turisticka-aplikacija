<template>
  <div class="container">

    <div v-if="loading" class="text-center">Loading tours...</div>
    
    <div v-else class="tours-grid">
      <div v-for="tour in tours" :key="tour.id" class="tour-card" @click="$router.push(`/tour/${tour.id}`)">
        
        <div class="card-bg" :style="{ backgroundImage: `url(${tour.imageUrl || 'https://via.placeholder.com/400x300?text=No+Image'})` }">
            
            <div class="overlay"></div>

            <div class="card-top">
                <span class="level-badge" :class="'diff-' + tour.difficulty">
                    Level {{ tour.difficulty }}
                </span>

                <div class="status-badge">
                    <span v-if="isPurchased(tour.id)" class="owned-tag">
                        <i class="fa fa-check-circle"></i> OWNED
                    </span>
                    <span v-else class="price-tag">
                        ${{ tour.price }}
                    </span>
                </div>
            </div>

            <div class="card-bottom">
                <h3 class="tour-title">{{ tour.name }}</h3>
                
                <div class="bottom-row">
                    <div class="tags">
                        <span v-for="tag in tour.tags.slice(0,2)" :key="tag" class="tag-pill">#{{ tag }}</span>
                    </div>

                    <button 
                        v-if="authStore.isTourist() && !isPurchased(tour.id)" 
                        @click.stop="addToCart(tour)" 
                        class="btn-cart-mini" 
                        title="Add to Cart"
                    >
                        <i class="fa fa-shopping-cart"></i>
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
import { useAuthStore } from '@/stores/auth'

const tourStore = useTourStore()
const cartStore = useCartStore()
const authStore = useAuthStore()
const tours = ref([])
const purchasedTourIds = ref(new Set())
const loading = ref(true)

onMounted(async () => {
  try {
    // 1. Učitaj sve ture
    tours.value = await tourStore.getAllTours()

    // 2. Ako je turista, učitaj kupljene ture da znamo status
    if (authStore.isTourist()) {
        try {
            const purchased = await tourStore.getPurchasedTours()
            purchased.forEach(t => purchasedTourIds.value.add(t.id))
        } catch (e) {
            console.log("Error checking purchases", e)
        }
    }
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
})

const isPurchased = (tourId) => {
    return purchasedTourIds.value.has(tourId)
}

const addToCart = async (tour) => {
    await cartStore.addToCart(tour)
}
</script>

<style scoped>
.container { max-width: 1200px; margin: 0 auto; padding: 40px 20px; }
.text-center { text-align: center; margin-top: 50px; color: #666; font-size: 1.2rem; }

.tours-grid { 
    display: grid; 
    grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); 
    gap: 30px; 
}

/* KARTICA STIL */
.tour-card { 
    height: 380px; 
    border-radius: 16px; 
    overflow: hidden; 
    cursor: pointer; 
    box-shadow: 0 10px 20px rgba(0,0,0,0.1); 
    transition: transform 0.3s, box-shadow 0.3s;
    position: relative;
    background: #000;
}

.tour-card:hover { 
    transform: translateY(-5px); 
    box-shadow: 0 15px 30px rgba(0,0,0,0.2); 
}

.card-bg {
    width: 100%; height: 100%;
    background-size: cover;
    background-position: center;
    position: relative;
    transition: transform 0.5s;
}
.tour-card:hover .card-bg { transform: scale(1.05); }

/* GRADIENT */
.overlay {
    position: absolute; inset: 0;
    background: linear-gradient(to bottom, rgba(0,0,0,0) 0%, rgba(0,0,0,0.1) 40%, rgba(0,0,0,0.9) 100%);
    z-index: 1;
}

/* TOP SECTION */
.card-top {
    position: absolute; top: 15px; left: 15px; right: 15px;
    display: flex; justify-content: space-between; z-index: 2;
}

.level-badge {
    padding: 5px 12px; border-radius: 20px; 
    color: white; font-weight: bold; font-size: 0.85rem;
    box-shadow: 0 2px 5px rgba(0,0,0,0.3);
    text-transform: uppercase;
}
/* BOJE ZA LEVEL (Iste u oba fajla) */
.diff-1 { background: #2ecc71; } 
.diff-2 { background: #f1c40f; color: #333; } 
.diff-3 { background: #e67e22; } 
.diff-4 { background: #e74c3c; } 
.diff-5 { background: #8e44ad; } 

.price-tag {
    background: white; color: #333; 
    padding: 5px 12px; border-radius: 8px; 
    font-weight: 800; font-size: 1rem;
    box-shadow: 0 2px 5px rgba(0,0,0,0.2);
}

.owned-tag {
    background: #27ae60; color: white;
    padding: 5px 12px; border-radius: 8px;
    font-weight: 700; font-size: 0.9rem;
    box-shadow: 0 2px 5px rgba(0,0,0,0.2);
    display: flex; align-items: center; gap: 5px;
}

/* BOTTOM SECTION */
.card-bottom {
    position: absolute; bottom: 20px; left: 20px; right: 20px;
    z-index: 2; text-align: left;
}

.tour-title {
    color: white; margin: 0 0 10px 0;
    font-size: 1.6rem; font-weight: 800;
    line-height: 1.2;
    text-shadow: 0 2px 4px rgba(0,0,0,0.5);
}

.bottom-row { display: flex; justify-content: space-between; align-items: center; }

.tag-pill { 
    color: #ddd; font-size: 0.85rem; font-weight: 500; margin-right: 8px; 
}

.btn-cart-mini {
    background: #cc072a; color: white; border: none;
    width: 36px; height: 36px; border-radius: 50%;
    display: flex; align-items: center; justify-content: center;
    cursor: pointer; transition: 0.2s; box-shadow: 0 4px 10px rgba(0,0,0,0.3);
}
.btn-cart-mini:hover { transform: scale(1.1); background: #a50522; }

.empty-state { text-align: center; color: #777; margin-top: 50px; }
</style>