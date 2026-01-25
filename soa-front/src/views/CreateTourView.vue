<template>
  <div class="container">
    <div class="create-tour-wrapper">
      
      <div class="form-section card">
        <h2>Create New Tour</h2>
        
        <form @submit.prevent="handleCreateTour">
          <div class="form-group">
            <label>Name</label>
            <input v-model="form.name" type="text" required placeholder="Ex. Walk through Petrovaradin" />
          </div>

          <div class="form-group">
            <label>Description</label>
            <textarea v-model="form.description" rows="3" required></textarea>
          </div>

          <div class="form-group">
            <label>Difficulty (1-5)</label>
            <input v-model.number="form.difficulty" type="number" min="1" max="5" required />
          </div>

          <div class="form-group">
            <label>Tags (comma separated)</label>
            <input v-model="tagsInput" type="text" placeholder="nature, history, city" />
          </div>

          <div class="form-group">
            <label>Price ($)</label>
            <input v-model.number="form.price" type="number" min="0" required />
          </div>

          <button type="submit" class="btn btn-primary" :disabled="loading || currentTourId">
            {{ loading ? 'Processing...' : (currentTourId ? 'Tour Created' : 'Create Tour & Add Points') }}
          </button>
        </form>

        <div v-if="currentTourId" class="success-box">
          <h3>Tour Created! ID: {{ currentTourId }}</h3>
          <p>Now click on the map to add Keypoints.</p>
          
          <button @click="finishTour" class="btn btn-success mt-10">
            ✅ Finish & Go to My Tours
          </button>
        </div>
      </div>

      <div class="map-section card">
        <h3>Map (Click to add Keypoints)</h3>
        <div id="map" style="height: 500px; width: 100%; z-index: 1;"></div>
        
        <div class="keypoints-list" v-if="keypoints.length > 0">
            <h4>Added Points:</h4>
            <ul>
                <li v-for="(kp, i) in keypoints" :key="i">
                    {{ kp.name }} ({{ kp.latitude.toFixed(4) }}, {{ kp.longitude.toFixed(4) }})
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
import { useRouter } from 'vue-router' // Dodali smo ruter
import L from 'leaflet'
import icon from 'leaflet/dist/images/marker-icon.png'
import iconShadow from 'leaflet/dist/images/marker-shadow.png'

const tourStore = useTourStore()
const router = useRouter() // Inicijalizacija rutera

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
    // Ne prikazujemo alert, samo se pojavljuje zeleni box
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

// Funkcija za kraj
const finishTour = () => {
    router.push('/my-tours')
}
</script>

<style scoped>
.create-tour-wrapper { display: flex; gap: 20px; flex-wrap: wrap; margin-top: 20px; }
.form-section { flex: 1; min-width: 300px; }
.map-section { flex: 1.5; min-width: 400px; }
.success-box { background: #d4edda; color: #155724; padding: 15px; margin-top: 15px; border-radius: 4px; border: 1px solid #c3e6cb; }
.keypoints-list { margin-top: 15px; background: #f8f9fa; padding: 10px; }
#map { z-index: 0; } 

.form-group { margin-bottom: 15px; }

input, textarea, select {
    width: 100%; 
    padding: 10px;
    box-sizing: border-box; 
    border: 1px solid #ddd;
    border-radius: 4px;
    font-size: 1rem;
    margin-top: 5px;
}

textarea {
    resize: vertical; 
    font-family: inherit;
}

button { width: 100%; margin-top: 10px; }

/* Stilovi za Finish dugme */
.mt-10 { margin-top: 15px; }
.btn-success {
    background-color: #28a745;
    color: white;
    border: none;
    font-weight: bold;
    padding: 12px;
    cursor: pointer;
    transition: background 0.3s;
}
.btn-success:hover {
    background-color: #218838;
}
</style>