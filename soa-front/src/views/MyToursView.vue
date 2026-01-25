<template>
  <div class="container">
    <div class="header-flex">
      <h2>My Tours Management</h2>
      <button @click="$router.push('/create-tour')" class="btn btn-primary">
        + Create New Tour
      </button>
    </div>

    <div v-if="loading" class="text-center">Loading your tours...</div>
    
    <div v-else class="card">
      <table class="tours-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Status</th>
            <th>Price</th>
            <th>Difficulty</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="tour in tours" :key="tour.id">
            <td><strong>{{ tour.name }}</strong></td>
            <td>
              <span class="badge" :class="getStatusClass(tour.status)">
                {{ getStatusName(tour.status) }}
              </span>
            </td>
            <td>${{ tour.price }}</td>
            <td>{{ tour.difficulty }}/5</td>
            <td class="actions">
               <button v-if="tour.status === 0" 
                       @click="changeStatus(tour, 1)" 
                       class="btn-sm btn-publish"
                       title="Publish to make it visible to tourists">
                 Publish
               </button>

               <button v-if="tour.status === 1" 
                       @click="changeStatus(tour, 2)" 
                       class="btn-sm btn-archive"
                       title="Hide from tourists">
                 Archive
               </button>

               <button v-if="tour.status === 2" 
                       @click="changeStatus(tour, 1)" 
                       class="btn-sm btn-publish">
                 Re-Publish
               </button>

               <button @click="deleteTour(tour.id)" class="btn-sm btn-delete">
                 Delete
               </button>
            </td>
          </tr>
        </tbody>
      </table>
      
      <div v-if="tours.length === 0" style="padding: 20px; text-align: center;">
        No tours found. Start by creating one!
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { useTourStore } from '@/stores/tours'
import { useAuthStore } from '@/stores/auth'
import api from '@/services/api' // Importujemo api direktno za brisanje

const tourStore = useTourStore()
const authStore = useAuthStore()
const tours = ref([])
const loading = ref(true)

// Funkcija za učitavanje tura
const loadTours = async () => {
  if (authStore.user?.id) {
    try {
        tours.value = await tourStore.getToursByAuthor(authStore.user.id)
    } catch (e) {
        console.error("Error loading tours:", e)
    }
  }
}

onMounted(async () => {
  try {
    await loadTours()
  } finally {
    loading.value = false
  }
})

// --- PROMENA STATUSA (Draft <-> Published <-> Archived) ---
const changeStatus = async (tour, newStatus) => {
    // Validacija za Publish (zahtev: mora imati bar 2 tačke)
    // Napomena: Ovo radi samo ako backend vraća keypoints listu u ovom view-u. 
    // Ako ne vraća, backend će svakako odbiti zahtev ako implementiraš tu logiku tamo.
    
    const actionName = newStatus === 1 ? 'Publish' : 'Archive';
    
    if(!confirm(`Are you sure you want to ${actionName} this tour?`)) return
    
    try {
        // Pozivamo akciju iz Store-a koju smo malopre dodali
        await tourStore.updateStatus(tour.id, newStatus)
        // Osvežavamo listu da se vidi promena boje/statusa
        await loadTours() 
    } catch (e) {
        alert('Failed to update status. ' + (e.response?.data || e.message))
    }
}

// --- BRISANJE TURE ---
const deleteTour = async (id) => {
    if(!confirm('Are you sure you want to DELETE this tour permanently?')) return
    
    try {
        // Direktno gađamo API endpoint koji smo upravo napravili
        await api.delete(`/tours/${id}`)
        // Sklanjamo turu iz liste bez ponovnog učitavanja cele stranice
        tours.value = tours.value.filter(t => t.id !== id)
    } catch (e) {
        console.error(e)
        alert('Failed to delete tour.')
    }
}

// Pomoćne funkcije za prikaz teksta i boja
const getStatusName = (s) => ['Draft', 'Published', 'Archived'][s] || 'Unknown'

const getStatusClass = (s) => {
    if (s === 0) return 'badge-draft' // Draft
    if (s === 1) return 'badge-pub'   // Published
    if (s === 2) return 'badge-arch'  // Archived
    return ''
}
</script>

<style scoped>
.header-flex { display: flex; justify-content: space-between; align-items: center; margin: 30px 0; }
.text-center { text-align: center; margin-top: 20px; font-size: 1.2rem; color: #666; }

.tours-table { width: 100%; border-collapse: collapse; min-width: 600px; }
.tours-table th, .tours-table td { padding: 12px 15px; text-align: left; border-bottom: 1px solid #eee; }
.tours-table th { background-color: #f8f9fa; color: #333; font-weight: 600; }

/* Status Badges */
.badge { padding: 5px 10px; border-radius: 20px; font-size: 0.85rem; color: white; font-weight: bold; text-transform: uppercase; letter-spacing: 0.5px; }
.badge-draft { background: #6c757d; }      /* Siva */
.badge-pub { background: #28a745; }        /* Zelena */
.badge-arch { background: #ffc107; color: #333; } /* Žuta/Narandžasta */

/* Actions Buttons */
.actions { display: flex; gap: 8px; flex-wrap: wrap; }
.btn-sm { padding: 6px 12px; border: none; cursor: pointer; border-radius: 4px; font-size: 0.85rem; color: white; transition: opacity 0.2s, transform 0.1s; font-weight: 500; }
.btn-sm:hover { opacity: 0.9; transform: translateY(-1px); }
.btn-sm:active { transform: translateY(0); }

.btn-publish { background-color: #28a745; }
.btn-archive { background-color: #ffc107; color: #333; }
.btn-delete { background-color: #dc3545; }

/* Primary Button */
.btn-primary { background-color: #007bff; color: white; border: none; padding: 10px 20px; border-radius: 5px; cursor: pointer; font-size: 1rem; }
.btn-primary:hover { background-color: #0056b3; }

/* Responsive table scrolling */
.card { overflow-x: auto; }
</style>