<template>
  <div class="container">
    <div class="create-tour-wrapper">
      
      <div class="form-section card">
        <div class="section-header">
            <h2>Create New Tour</h2>
        </div>
        
        <form @submit.prevent="handleCreateTour">
          
          <div class="image-preview-container" @click="showImageModal = true">
            <img v-if="form.imageUrl" :src="form.imageUrl" class="cover-preview" />
            <div v-else class="placeholder-preview">
                <i class="fa fa-camera"></i>
                <span>Add Cover Photo</span>
            </div>
            <div v-if="form.imageUrl" class="edit-overlay">
                <i class="fa fa-pen"></i> Change
            </div>
          </div>

          <div class="form-group">
            <label>Name</label>
            <input v-model="form.name" type="text" required placeholder="Ex. Walk through Petrovaradin" />
          </div>

          <div class="form-group">
            <label>Description</label>
            <textarea v-model="form.description" rows="3" required></textarea>
          </div>

          <div class="form-row">
             <div class="form-group full-width">
                <label>Difficulty (1-5)</label>
                <input v-model.number="form.difficulty" type="number" min="1" max="5" required />
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
          <div class="success-icon"><i class="fa fa-check-circle"></i></div>
          <h3>Tour Created!</h3>
          <p class="instruction">Now click on the map to add Keypoints.</p>
          <button @click="finishTour" class="btn btn-finish">
            Finish & Save <i class="fa fa-arrow-right"></i>
          </button>
        </div>
      </div>

      <div class="map-section card">
        <div class="section-header">
            <h3>Map <small>(Click map to add a point)</small></h3>
        </div>
        <div id="map"></div>
        
        <div class="keypoints-container" v-if="keypoints.length > 0">
            <h4><i class="fa fa-list-ul"></i> Added Points ({{ keypoints.length }})</h4>
            <ul class="kp-list">
                <li v-for="(kp, i) in keypoints" :key="i" class="kp-item">
                    <div class="kp-number">{{ i + 1 }}</div>
                    <div class="kp-content">
                        <span class="kp-name">{{ kp.name }}</span>
                        <small class="kp-desc">{{ kp.description }}</small>
                        <img v-if="kp.image" :src="kp.image" class="kp-thumb-small" />
                    </div>
                </li>
            </ul>
        </div>
      </div>

    </div>

    <div v-if="showImageModal" class="modal-overlay" @click.self="showImageModal = false">
        <div class="modal-content">
            <h3>Set Cover Photo</h3>
            <input v-model="tempImageUrl" type="text" placeholder="Paste image URL here..." class="modal-input" />
            <div class="modal-actions">
                <button @click="showImageModal = false" class="btn-cancel">Cancel</button>
                <button @click="saveImage" class="btn-confirm">Set Image</button>
            </div>
        </div>
    </div>

    <div v-if="showKeypointModal" class="modal-overlay" @click.self="closeKeypointModal">
        <div class="modal-content">
            <h3>Add Keypoint</h3>
            <p class="coords-info">Coordinates: {{ tempKeypoint.lat.toFixed(4) }}, {{ tempKeypoint.lng.toFixed(4) }}</p>
            
            <div class="form-group">
                <label>Point Name</label>
                <input v-model="tempKeypoint.name" type="text" placeholder="e.g. The Museum" class="modal-input" />
            </div>

            <div class="form-group">
                <label>Description</label>
                <textarea v-model="tempKeypoint.description" rows="2" placeholder="Short info..." class="modal-input"></textarea>
            </div>

            <div class="form-group">
                <label>Image URL</label>
                <input v-model="tempKeypoint.image" type="text" placeholder="https://..." class="modal-input" />
            </div>

            <div class="modal-actions">
                <button @click="closeKeypointModal" class="btn-cancel">Cancel</button>
                <button @click="confirmAddKeypoint" class="btn-confirm">Add Point</button>
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
import 'leaflet/dist/leaflet.css'

const tourStore = useTourStore()
const router = useRouter()

let DefaultIcon = L.icon({
    iconUrl: icon, shadowUrl: iconShadow,
    iconSize: [25, 41], iconAnchor: [12, 41], popupAnchor: [1, -34]
});
L.Marker.prototype.options.icon = DefaultIcon;

// FORMA BEZ PRICE-a
const form = reactive({
  name: '', description: '', difficulty: 1, status: 0, imageUrl: ''
})
const tagsInput = ref('')
const loading = ref(false)
const currentTourId = ref(null)
const keypoints = ref([])
let map = null 

// Modal states
const showImageModal = ref(false)
const tempImageUrl = ref('')
const showKeypointModal = ref(false)
const tempKeypoint = reactive({ name: '', description: '', image: '', lat: 0, lng: 0 })

onMounted(() => {
    map = L.map('map').setView([45.2671, 19.8335], 13);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { attribution: '&copy; OpenStreetMap' }).addTo(map);
    map.on('click', onMapClick);
})

const saveImage = () => {
    form.imageUrl = tempImageUrl.value
    showImageModal.value = false
}

const handleCreateTour = async () => {
  loading.value = true
  try {
    const payload = {
      ...form,
      tags: tagsInput.value.split(',').map(t => t.trim()),
      authorId: 1,
      price: 0 // Šaljemo 0 jer se cena dodaje tek kod Publish-a
    }
    const res = await tourStore.createTour(payload)
    currentTourId.value = res.id 
  } catch (err) {
    alert('Error creating tour')
  } finally {
    loading.value = false
  }
}

const onMapClick = (e) => {
    if (!currentTourId.value) {
        alert('Please create the tour details first (click the black button)!')
        return
    }
    tempKeypoint.lat = e.latlng.lat
    tempKeypoint.lng = e.latlng.lng
    tempKeypoint.name = ''
    tempKeypoint.description = ''
    tempKeypoint.image = ''
    showKeypointModal.value = true
}

const closeKeypointModal = () => { showKeypointModal.value = false }

const confirmAddKeypoint = async () => {
    if (!tempKeypoint.name) { alert("Enter name"); return; }
    const newPoint = {
        name: tempKeypoint.name,
        description: tempKeypoint.description || "",
        image: tempKeypoint.image || "", 
        latitude: tempKeypoint.lat,
        longitude: tempKeypoint.lng
    }
    try {
        await tourStore.addKeypoint(currentTourId.value, newPoint)
        keypoints.value.push({ ...newPoint })
        L.marker([newPoint.latitude, newPoint.longitude]).addTo(map).bindPopup(`<b>${newPoint.name}</b>`).openPopup();
        showKeypointModal.value = false
    } catch (err) { alert('Failed to save keypoint') }
}

const finishTour = () => { router.push('/my-tours') }
</script>

<style scoped>
/* Iste stilove kao pre */
.create-tour-wrapper { display: flex; gap: 25px; flex-wrap: wrap; margin-top: 20px; align-items: stretch; }
.form-section { flex: 1; min-width: 320px; padding: 25px; }
.map-section { flex: 1.5; min-width: 400px; padding: 0; display: flex; flex-direction: column; }
#map { width: 100%; height: 400px; z-index: 0; }
.section-header { margin-bottom: 20px; border-bottom: 2px solid #f0f0f0; padding-bottom: 10px; }
.image-preview-container { width: 100%; height: 180px; border-radius: 8px; background: #f0f2f5; border: 2px dashed #ccc; display: flex; align-items: center; justify-content: center; cursor: pointer; overflow: hidden; position: relative; margin-bottom: 20px; transition: 0.2s; }
.image-preview-container:hover { border-color: #cc072a; background: #fff5f5; }
.placeholder-preview { display: flex; flex-direction: column; align-items: center; color: #888; }
.cover-preview { width: 100%; height: 100%; object-fit: cover; }
.edit-overlay { position: absolute; bottom: 0; left: 0; right: 0; background: rgba(0,0,0,0.6); color: white; padding: 5px; text-align: center; font-size: 0.8rem; }
.form-group { margin-bottom: 15px; }
.form-row { display: flex; gap: 15px; }
.form-row .half { flex: 1; }
.form-row .full-width { flex: 1; }
label { font-weight: 600; font-size: 0.9rem; color: #555; display: block; margin-bottom: 5px; }
input, textarea { width: 100%; padding: 10px; border: 1px solid #ddd; border-radius: 6px; font-size: 1rem; box-sizing: border-box; }
input:focus, textarea:focus { border-color: #cc072a; outline: none; }
.btn-create { width: 100%; background: #333; color: white; border: none; padding: 12px; border-radius: 6px; cursor: pointer; font-weight: bold; }
.btn-create:hover:not(:disabled) { background: #000; }
.btn-create:disabled { background: #ccc; cursor: not-allowed; }
.success-box { background: #e6fcf5; border: 1px solid #20c997; padding: 20px; margin-top: 25px; border-radius: 8px; text-align: center; }
.success-icon { font-size: 3rem; color: #20c997; margin-bottom: 10px; }
.btn-finish { background-color: #20c997; color: white; border: none; font-weight: bold; padding: 12px 25px; border-radius: 6px; cursor: pointer; width: 100%; margin-top: 10px; }
.keypoints-container { padding: 15px; background: #fafafa; border-top: 1px solid #eee; flex-grow: 1; overflow-y: auto; }
.kp-item { display: flex; align-items: flex-start; gap: 12px; background: white; padding: 10px; margin-bottom: 8px; border-radius: 6px; box-shadow: 0 1px 3px rgba(0,0,0,0.05); border-left: 3px solid #cc072a; }
.kp-number { background: #cc072a; color: white; width: 24px; height: 24px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 0.8rem; font-weight: bold; flex-shrink: 0; margin-top: 2px;}
.kp-content { flex: 1; display: flex; flex-direction: column; }
.kp-name { font-weight: 600; color: #2c3e50; font-size: 0.95rem; }
.kp-thumb-small { width: 100%; max-height: 100px; object-fit: cover; border-radius: 4px; margin-top: 5px; }
.modal-overlay { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 2000; }
.modal-content { background: white; padding: 25px; border-radius: 12px; width: 90%; max-width: 450px; text-align: left; box-shadow: 0 10px 30px rgba(0,0,0,0.2); }
.modal-input { width: 100%; padding: 10px; border: 1px solid #ddd; border-radius: 6px; font-size: 1rem; margin-top: 0; }
.modal-actions { display: flex; gap: 10px; justify-content: flex-end; margin-top: 20px; }
.btn-cancel { background: #eee; border: none; padding: 10px 20px; border-radius: 6px; cursor: pointer; }
.btn-confirm { background: #cc072a; color: white; border: none; padding: 10px 20px; border-radius: 6px; cursor: pointer; }
</style>