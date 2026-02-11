<template>
  <div class="home-container">
    
    <div class="hero-section">
      <div v-for="(slide, index) in slides" :key="index" 
           class="hero-slide" 
           :class="{ active: currentSlide === index }">
        <div class="overlay"></div>
        <img :src="slide.image" :alt="slide.title" class="slide-img">
        
        <div class="hero-content">
          <h1 class="animate-up">{{ slide.title }}</h1>
          <p class="animate-up delay">{{ slide.subtitle }}</p>
          <button @click="$router.push('/tours')" class="btn-explore animate-up delay-2">
            Explore Tours <i class="fa fa-arrow-right"></i>
          </button>
        </div>
      </div>

      <div class="indicators">
        <span v-for="(slide, index) in slides" :key="index" 
              class="dot" 
              :class="{ active: currentSlide === index }"
              @click="currentSlide = index">
        </span>
      </div>
    </div>

    <div class="featured-container container">
      <div class="section-header">
        <h2>Popular Adventures</h2>
        <p>Check out our latest and greatest tours chosen just for you.</p>
      </div>

      <div v-if="loading" class="text-center">Loading recommendations...</div>

      <div v-else class="tours-grid">
  <div v-for="tour in featuredTours" :key="tour.id" class="tour-card" @click="$router.push(`/tour/${tour.id}`)">
    
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
                <div class="tags" v-if="tour.tags">
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
      
      <div class="view-all-wrapper">
          <button @click="$router.push('/tours')" class="btn-view-all">View All Tours</button>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { useTourStore } from '@/stores/tours'
import { useCartStore } from '@/stores/cart'
import { useAuthStore } from '@/stores/auth'

const tourStore = useTourStore()
const cartStore = useCartStore()
const authStore = useAuthStore()

const tours = ref([])
const purchasedTourIds = ref(new Set())
const loading = ref(true)
const currentSlide = ref(0)
let slideInterval = null

const slides = [
    {
        image: 'https://images.unsplash.com/photo-1469854523086-cc02fe5d8800?q=80&w=2021&auto=format&fit=crop',
        title: 'Discover the Unseen',
        subtitle: 'Journey to the most breathtaking places on Earth.'
    },
    {
        image: 'https://images.unsplash.com/photo-1476514525535-07fb3b4ae5f1?q=80&w=2070&auto=format&fit=crop',
        title: 'Adventure Awaits',
        subtitle: 'Experience nature like never before with our guided tours.'
    },
    {
        image: 'https://images.unsplash.com/photo-1501785888041-af3ef285b470?q=80&w=2070&auto=format&fit=crop',
        title: 'Create Memories',
        subtitle: 'Travela makes your dream vacation a reality.'
    }
]

// --- COMPUTED: PRVIH 5 TURA ---
const featuredTours = computed(() => {
    return tours.value.slice(0, 5)
})

// --- LIFECYCLE ---
onMounted(async () => {
    startSlideTimer()
    
    try {
        if (tourStore.tours.length === 0) {
            await tourStore.getAllTours()
        }
        tours.value = tourStore.tours

        if (authStore.isTourist()) {
            try {
                const purchased = await tourStore.getPurchasedTours()
                purchased.forEach(t => purchasedTourIds.value.add(t.id))
            } catch (e) {
                console.log("Error checking purchases", e)
            }
        }
    } catch (e) {
        console.error(e)
    } finally {
        loading.value = false
    }
})

onUnmounted(() => {
    stopSlideTimer()
})



const startSlideTimer = () => {
    slideInterval = setInterval(() => {
        currentSlide.value = (currentSlide.value + 1) % slides.length
    }, 5000) // Menja se na 5 sekundi
}

const stopSlideTimer = () => {
    if (slideInterval) clearInterval(slideInterval)
}

const truncateText = (text, length) => {
  if (!text) return ''
  return text.length > length ? text.substring(0, length) + '...' : text
}

const isPurchased = (tourId) => {
    return purchasedTourIds.value.has(tourId)
}

const addToCart = async (tour) => {
    await cartStore.addToCart(tour)
}
</script>

<style scoped>
.home-container { padding-bottom: 50px; }

/* --- HERO SECTION --- */
.hero-section {
    position: relative;
    width: 100%;
    height: 80vh; 
    overflow: hidden;
    color: white;
}

.hero-slide {
    position: absolute;
    top: 0; left: 0; width: 100%; height: 100%;
    opacity: 0;
    transition: opacity 1s ease-in-out;
    display: flex;
    align-items: center;
    justify-content: center;
    text-align: center;
}

.hero-slide.active { opacity: 1; }

.slide-img {
    position: absolute;
    top: 0; left: 0; width: 100%; height: 100%;
    object-fit: cover;
    z-index: -2;
}

/* Tamni sloj preko slike da se tekst bolje vidi */
.overlay {
    position: absolute;
    top: 0; left: 0; width: 100%; height: 100%;
    background: rgba(0, 0, 0, 0.4);
    z-index: -1;
}

.hero-content h1 { font-size: 3.5rem; font-weight: 800; text-shadow: 2px 2px 10px rgba(0,0,0,0.5); margin-bottom: 10px; }
.hero-content p { font-size: 1.5rem; margin-bottom: 30px; text-shadow: 1px 1px 5px rgba(0,0,0,0.5); }

/* Dugme Explore */
.btn-explore {
    padding: 15px 40px;
    font-size: 1.2rem;
    font-weight: bold;
    color: white;
    background: #cc072a;
    border: none;
    border-radius: 50px;
    cursor: pointer;
    transition: 0.3s;
    box-shadow: 0 4px 15px rgba(0,0,0,0.3);
}
.btn-explore:hover {
    background: #a80622;
    transform: translateY(-3px);
}

/* Indikatori (tačkice) */
.indicators {
    position: absolute; bottom: 20px; left: 50%; transform: translateX(-50%);
    display: flex; gap: 10px; z-index: 10;
}
.dot { width: 12px; height: 12px; background: rgba(255,255,255,0.5); border-radius: 50%; cursor: pointer; transition: 0.3s; }
.dot.active { background: white; transform: scale(1.2); }

/* Animacije teksta */
.animate-up { transform: translateY(30px); opacity: 0; transition: 0.8s ease-out; }
.hero-slide.active .animate-up { transform: translateY(0); opacity: 1; }
.delay { transition-delay: 0.2s; }
.delay-2 { transition-delay: 0.4s; }


/* --- FEATURED TOURS --- */
.featured-container { margin-top: 60px; }
.section-header { text-align: center; margin-bottom: 40px; }
.section-header h2 { font-size: 2.5rem; color: #2c3e50; margin-bottom: 10px; }
.section-header p { color: #777; font-size: 1.1rem; }

/* Grid (Sličan kao ToursView, malo uprošćen) */
.tours-grid { 
    display: grid; 
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr)); 
    gap: 30px; 
}

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
.tour-card:hover { transform: translateY(-5px); box-shadow: 0 15px 30px rgba(0,0,0,0.2); }

.card-bg {
    width: 100%; height: 100%;
    background-size: cover; background-position: center;
    position: relative; transition: transform 0.5s;
}
.tour-card:hover .card-bg { transform: scale(1.05); }

.overlay {
    position: absolute; inset: 0;
    background: linear-gradient(to bottom, rgba(0,0,0,0) 0%, rgba(0,0,0,0.1) 40%, rgba(0,0,0,0.9) 100%);
    z-index: 1;
}

.card-top {
    position: absolute; top: 15px; left: 15px; right: 15px;
    display: flex; justify-content: space-between; z-index: 2;
}

.level-badge {
    padding: 5px 12px; border-radius: 20px; 
    color: white; font-weight: bold; font-size: 0.85rem;
    box-shadow: 0 2px 5px rgba(0,0,0,0.3); text-transform: uppercase;
}
/* Boje težine */
.diff-1 { background: #2ecc71; } 
.diff-2 { background: #f1c40f; color: #333; } 
.diff-3 { background: #e67e22; } 
.diff-4 { background: #e74c3c; } 
.diff-5 { background: #8e44ad; } 

.price-tag {
    background: white; color: #333; 
    padding: 5px 12px; border-radius: 8px; 
    font-weight: 800; font-size: 1rem; box-shadow: 0 2px 5px rgba(0,0,0,0.2);
}
.owned-tag {
    background: #27ae60; color: white; padding: 5px 12px; border-radius: 8px;
    font-weight: 700; font-size: 0.9rem; display: flex; align-items: center; gap: 5px;
}

.card-bottom {
    position: absolute; bottom: 20px; left: 20px; right: 20px; z-index: 2; text-align: left;
}

.tour-title {
    color: white; margin: 0 0 10px 0; font-size: 1.6rem; font-weight: 800;
    text-shadow: 0 2px 4px rgba(0,0,0,0.5);
}

.bottom-row { display: flex; justify-content: space-between; align-items: center; }
.tag-pill { color: #ddd; font-size: 0.85rem; font-weight: 500; margin-right: 8px; }

.btn-cart-mini {
    background: #cc072a; color: white; border: none; width: 36px; height: 36px;
    border-radius: 50%; display: flex; align-items: center; justify-content: center;
    cursor: pointer; transition: 0.2s;
}
.btn-cart-mini:hover { transform: scale(1.1); background: #a50522; }
.btn-view-all {
    padding: 12px 30px; background: #2c3e50; color: white; border: none;
    border-radius: 6px; font-size: 1rem; cursor: pointer; transition: 0.2s;
}
.btn-view-all:hover { background: #1a252f; }
.view-all-wrapper {
    margin: 60px 0 20px 0;
    display: flex;
    justify-content: center;
}

</style>