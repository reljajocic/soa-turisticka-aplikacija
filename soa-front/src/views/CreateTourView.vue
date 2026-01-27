<template>
  <div class="container">
    <div class="create-tour-wrapper">
      
      <div class="form-section card">
        <div class="section-header">
            <h2>Create New Tour</h2>
        </div>
        
        <form @submit.prevent="handleCreateTour">
          <div class="form-group">
            <label>Name</label>
            <input v-model="form.name" type="text" required placeholder="Ex. Walk through Petrovaradin" />
          </div>

          <div class="form-group">
            <label>Description</label>
            <textarea v-model="form.description" rows="3" required></textarea>
          </div>

          <div class="form-row">
             <div class="form-group half">
                <label>Difficulty (1-5)</label>
                <input v-model.number="form.difficulty" type="number" min="1" max="5" required />
             </div>
             <div class="form-group half">
                <label>Price ($)</label>
                <input v-model.number="form.price" type="number" min="0" required />
             </div>
          </div>

          <div class="form-group">
            <label>Tags (comma separated)</label>
            <input v-model="tagsInput" type="text" placeholder="nature, history, city" />
          </div>

          <button type="submit" class="btn btn-create" :disabled="loading || currentTourId">
            {{ loading ? 'Processing...' : (currentTourId ? 'Tour Created' : 'Create Tour & Add Points') }}
          </button>
        </form>

        <div v-if="currentTourId" class="success-box">
          <div class="success-icon">
            <i class="fa fa-check-circle"></i>
          </div>
          <h3>Tour Created Successfully!</h3>
          
          <div class="tour-id-wrapper">
             <span class="id-label">Tour ID:</span>
             <code class="id-code">{{ currentTourId }}</code>
          </div>
          
          <p class="instruction"><i class="fa fa-map-marker-alt"></i> Now click on the map to add Keypoints.</p>
          
          <button @click="finishTour" class="btn btn-finish">
            Finish & Go to My Tours <i class="fa fa-arrow-right"></i>
          </button>
        </div>
      </div>

      <div class="map-section card">
        <div class="section-header">
            <h3>Map <small>(Click to add points)</small></h3>
        </div>
        
        <div id="map"></div>
        
        <div class="keypoints-container" v-if="keypoints.length > 0">
            <h4><i class="fa fa-list-ul"></i> Added Points ({{ keypoints.length }})</h4>
            
            <ul class="kp-list">
                <li v-for="(kp, i) in keypoints" :key="i" class="kp-item">
                    <div class="kp-number">{{ i + 1 }}</div>
                    <div class="kp-content">
                        <span class="kp-name">{{ kp.name }}</span>
                        <span class="kp-coords">{{ kp.latitude.toFixed(4) }}, {{ kp.longitude.toFixed(4) }}</span>
                    </div>
                </li>
            </ul>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useTourStore } from '@/stores/tours'
import { useRouter } from 'vue-router'
import L from 'leaflet'
import icon from 'leaflet/dist/images/marker-icon.png'
import iconShadow from 'leaflet/dist/images/marker-shadow.png'

const tourStore = useTourStore()
const router = useRouter()

// --- FIX ZA IKONICE ---
let DefaultIcon = L.icon({
    iconUrl: icon,
    shadowUrl: iconShadow,
    iconSize: [25, 41],
    iconAnchor: [12, 41]
});
L.Marker.prototype.options.icon = DefaultIcon;
// ----------------------

// Promenljive
const form = reactive({
  name: '', description: '', difficulty: 1, price: 0, status: 0
})
const tagsInput = ref('')
const loading = ref(false)
const currentTourId = ref(null)
const keypoints = ref([])
let map = null 

// --- INICIJALIZACIJA MAPE ---
onMounted(() => {
    map = L.map('map').setView([45.2671, 19.8335], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(map);

    map.on('click', onMapClick);
})

// --- LOGIKA ---
const handleCreateTour = async () => {
  loading.value = true
  try {
    const payload = {
      ...form,
      tags: tagsInput.value.split(',').map(t => t.trim()),
      authorId: 1 // Ovo bi idealno trebalo vući iz authStore.user.id
    }
    const res = await tourStore.createTour(payload)
    currentTourId.value = res.id 
  } catch (err) {
    console.error(err)
    alert('Error creating tour')
  } finally {
    loading.value = false
  }
}

const onMapClick = async (e) => {
    if (!currentTourId.value) {
        alert('Please create the tour details first (click the blue button)!')
        return
    }

    const { lat, lng } = e.latlng
    const name = prompt("Enter name for this keypoint:", `Point ${keypoints.value.length + 1}`)
    if (!name) return

    const newPoint = {
        name: name,
        description: "Default desc",
        image: "default.jpg",
        latitude: lat,
        longitude: lng
    }

    try {
        await tourStore.addKeypoint(currentTourId.value, newPoint)
        keypoints.value.push(newPoint)
        
        L.marker([lat, lng]).addTo(map)
            .bindPopup(`<b>${name}</b>`)
            .openPopup();
            
    } catch (err) {
        alert('Failed to save keypoint')
    }
}

const finishTour = () => {
    router.push('/my-tours')
}
</script>

<style scoped>
.create-tour-wrapper { 
    display: flex; 
    gap: 25px; 
    flex-wrap: wrap; 
    margin-top: 20px; 
    align-items: stretch; 
}
.form-section { flex: 1; min-width: 320px; padding: 25px; }
.map-section { 
    flex: 1.5; 
    min-width: 400px; 
    padding: 0; 
    overflow: hidden;
    display: flex;          
    flex-direction: column; 
}

#map { 
    width: 100%; 
    height: 400px; 
    flex-shrink: 0; 
    z-index: 0;
}

.section-header { margin-bottom: 20px; border-bottom: 2px solid #f0f0f0; padding-bottom: 10px; }
.section-header h2, .section-header h3 { margin: 0; color: #333; }
.section-header small { color: #666; font-size: 0.9rem; font-weight: normal; }

/* FORMA */
.form-group { margin-bottom: 15px; }
.form-row { display: flex; gap: 15px; }
.form-row .half { flex: 1; }

label { font-weight: 600; font-size: 0.9rem; color: #555; display: block; margin-bottom: 5px; }

input, textarea {
    width: 100%; 
    padding: 10px;
    box-sizing: border-box; 
    border: 1px solid #ddd;
    border-radius: 6px;
    font-size: 1rem;
    transition: border-color 0.3s;
}
input:focus, textarea:focus { border-color: #cc072a; outline: none; }
textarea { resize: vertical; font-family: inherit; }

/* DUGME CREATE */
.btn-create { 
    width: 100%; 
    background: #333; 
    color: white; 
    border: none; 
    padding: 12px; 
    border-radius: 6px; 
    cursor: pointer; 
    font-weight: 600; 
    transition: 0.2s;
}
.btn-create:hover:not(:disabled) { background: #000; }
.btn-create:disabled { background: #ccc; cursor: not-allowed; }

/* SUCCESS BOX STYLING */
.success-box { 
    background: #fff5f5; /* Veoma svetlo crvena */
    border: 1px solid #ffccd5;
    padding: 20px; 
    margin-top: 25px; 
    border-radius: 8px; 
    text-align: center;
    animation: fadeIn 0.5s ease-in-out;
}

.success-icon { font-size: 3rem; color: #28a745; margin-bottom: 10px; } /* Zelena kvačica */

.success-box h3 { color: #2c3e50; margin: 0 0 15px 0; font-size: 1.3rem; }

.tour-id-wrapper { 
    background: white; 
    padding: 8px 15px; 
    border-radius: 4px; 
    display: inline-block; 
    border: 1px dashed #ccc;
    margin-bottom: 15px;
}
.id-label { font-weight: bold; color: #666; margin-right: 5px; font-size: 0.9rem; }
.id-code { font-family: monospace; color: #cc072a; font-size: 1.1rem; font-weight: bold; }

.instruction { color: #555; margin-bottom: 20px; font-style: italic; }

.btn-finish {
    background-color: #cc072a;
    color: white;
    border: none;
    font-weight: bold;
    padding: 12px 25px;
    border-radius: 6px;
    cursor: pointer;
    transition: all 0.2s;
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 10px;
}
.btn-finish:hover { background-color: #99051f; transform: translateY(-2px); box-shadow: 0 4px 8px rgba(0,0,0,0.1); }

/* KEYPOINTS LISTA */
.map-section .section-header { padding: 15px 20px 0; border: none; margin-bottom: 5px; }

.keypoints-container { 
    padding: 15px; 
    background: #fafafa; 
    border-top: 1px solid #eee; 
    flex-grow: 1;  /* OVO JE BITNO: Uzima sav preostali prostor do dna kartice */
    display: flex;
    flex-direction: column;
    min-height: 0; /* Trik za flexbox skrol */
}
.keypoints-container h4 { margin: 0 0 15px 0; font-size: 1rem; color: #333; }

.kp-list { 
    list-style: none; 
    padding: 0; 
    margin: 0; 
    overflow-y: auto; /* Skroluje se samo lista ako ima previše tačaka */
    flex-grow: 1;     /* Širi se da popuni kontejner */
    height: 100%;     /* Osigurava punu visinu */
    /* max-height: 200px;  <-- OVO OBRIŠI, više nam ne treba limit */
}
.kp-item { 
    display: flex; 
    align-items: center; 
    gap: 12px; 
    background: white; 
    padding: 10px; 
    margin-bottom: 8px; 
    border-radius: 6px; 
    box-shadow: 0 1px 3px rgba(0,0,0,0.05); 
    border-left: 3px solid #cc072a;
}
.kp-number {
    background: #cc072a;
    color: white;
    width: 24px;
    height: 24px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.8rem;
    font-weight: bold;
    flex-shrink: 0;
}
.kp-content { display: flex; flex-direction: column; }
.kp-name { font-weight: 600; color: #2c3e50; font-size: 0.95rem; }
.kp-coords { font-size: 0.75rem; color: #888; font-family: monospace; }

@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }
</style>