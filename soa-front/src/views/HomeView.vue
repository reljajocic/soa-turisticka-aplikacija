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
        <div v-for="tour in featuredTours" :key="tour.id" class="tour-card">
          <div class="card-header" :class="'diff-' + tour.difficulty">
            <span class="difficulty-badge">Level {{ tour.difficulty }}</span>
          </div>

          <div class="card-body">
            <h3>{{ tour.name }}</h3>
            <p class="price">${{ tour.price }}</p>
            <p class="description">{{ truncateText(tour.description, 60) }}</p>
            
            <button @click="$router.push(`/tour/${tour.id}`)" class="btn-details">
              View Details
            </button>
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

const tourStore = useTourStore()
const tours = ref([])
const loading = ref(true)
const currentSlide = ref(0)
let slideInterval = null

// --- PODACI ZA SLAJDER ---
// Koristim Unsplash slike kao placeholder
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
    // 1. Pokreni slajder
    startSlideTimer()
    
    // 2. Učitaj ture
    try {
        // Ako već nisu učitane u store-u, povuci ih
        if (tourStore.tours.length === 0) {
            await tourStore.getAllTours()
        }
        tours.value = tourStore.tours
    } catch (e) {
        console.error(e)
    } finally {
        loading.value = false
    }
})

onUnmounted(() => {
    stopSlideTimer()
})

// --- LOGIKA SLAJDERA ---
const startSlideTimer = () => {
    slideInterval = setInterval(() => {
        currentSlide.value = (currentSlide.value + 1) % slides.length
    }, 5000) // Menja se na 5 sekundi
}

const stopSlideTimer = () => {
    if (slideInterval) clearInterval(slideInterval)
}

// --- HELPER ---
const truncateText = (text, length) => {
  if (!text) return ''
  return text.length > length ? text.substring(0, length) + '...' : text
}
</script>

<style scoped>
.home-container { padding-bottom: 50px; }

/* --- HERO SECTION --- */
.hero-section {
    position: relative;
    width: 100%;
    height: 80vh; /* Zauzima 80% visine ekrana */
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
    background: white; border-radius: 12px; overflow: hidden;
    box-shadow: 0 5px 15px rgba(0,0,0,0.08); transition: 0.3s;
    display: flex; flex-direction: column;
}
.tour-card:hover { transform: translateY(-10px); box-shadow: 0 15px 30px rgba(0,0,0,0.15); }

.card-header { height: 120px; background: #ddd; position: relative; }
.diff-1 { background: #cc072a; } 
.diff-2 { background: #cc072a; } 
.diff-3 { background: #a80622; } 
.diff-4 { background: #85051b; } 
.diff-5 { background: #610313; } 

.difficulty-badge {
    position: absolute; bottom: 10px; right: 10px;
    background: rgba(0,0,0,0.7); color: white; padding: 4px 10px;
    border-radius: 12px; font-size: 0.8rem; font-weight: bold;
}

.card-body { padding: 20px; flex-grow: 1; display: flex; flex-direction: column; text-align: center; }
.card-body h3 { font-size: 1.2rem; margin-bottom: 5px; color: #333; }
.price { color: #cc072a; font-weight: 800; font-size: 1.3rem; margin-bottom: 10px; }
.description { font-size: 0.9rem; color: #666; margin-bottom: 20px; flex-grow: 1; }

.btn-details {
    padding: 10px 20px; border: 2px solid #cc072a; color: #cc072a; background: transparent;
    border-radius: 6px; font-weight: bold; cursor: pointer; transition: 0.2s;
}
.btn-details:hover { background: #cc072a; color: white; }

.view-all-wrapper { text-align: center; margin-top: 50px; }
.btn-view-all {
    padding: 12px 30px; background: #2c3e50; color: white; border: none;
    border-radius: 6px; font-size: 1rem; cursor: pointer; transition: 0.2s;
}
.btn-view-all:hover { background: #1a252f; }
</style>