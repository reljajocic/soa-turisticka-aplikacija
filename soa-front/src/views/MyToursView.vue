<template>
  <div class="container">
    <div class="header-section">
      <template v-if="isAuthor">
          <h2>My Created Tours</h2>
          <button @click="$router.push('/create-tour')" class="btn btn-primary">
            <i class="fa fa-plus"></i> Create New Tour
          </button>
      </template>

      <template v-else>
          <h2>My Purchased Tours</h2>
          <button @click="$router.push('/tours')" class="btn btn-primary">
            Explore More Tours
          </button>
      </template>
    </div>

    <div v-if="loading" class="text-center">Loading tours...</div>
    
    <div v-else-if="tours.length === 0" class="empty-state">
      <i class="fa fa-map-signs empty-icon"></i>
      <h3>No tours found.</h3>
      <p v-if="isAuthor">You haven't created any tours yet.</p>
      <p v-else>You haven't purchased any tours yet.</p>
    </div>

    <div v-else class="tours-grid">
      <div v-for="tour in tours" :key="tour.id" 
           class="tour-card" 
           :class="{ 'tour-completed': !isAuthor && getExecStatus(tour.id).isFinished }"
           @click="$router.push(`/tour/${tour.id}`)">
        
        <div class="card-header" :class="'diff-' + tour.difficulty">
          <span class="difficulty-badge">Level {{ tour.difficulty }}</span>
          
          <span v-if="isAuthor" class="status-badge" :class="'status-' + tour.status">
             {{ getStatusName(tour.status) }}
          </span>

          <span v-else-if="getExecStatus(tour.id).label" 
                class="status-badge exec-badge" 
                :class="getExecStatus(tour.id).class">
             {{ getExecStatus(tour.id).label }}
          </span>
        </div>

        <div class="card-body">
          <h3>{{ tour.name }}</h3>
          
          <p class="tags">
            <span v-for="tag in tour.tags" :key="tag" class="tag">#{{ tag }}</span>
          </p>

          <p class="description">{{ truncateText(tour.description, 80) }}</p>

          <p v-if="!isAuthor && getExecStatus(tour.id).lastDate" class="last-finished">
            <i class="fa fa-clock-rotate-left"></i> Last: {{ formatDate(getExecStatus(tour.id).lastDate) }}
          </p>
          
          <div class="card-footer">
            <span class="price" v-if="tour.price > 0">${{ tour.price }}</span>
            <span class="price-draft" v-else>Not set</span>
            
            <div class="buttons">
                <template v-if="isAuthor">
                    <button v-if="tour.status === 0 || tour.status === 2" 
                            @click.stop="openPublishModal(tour)" class="btn btn-publish"><i class="fa fa-upload"></i></button>
                    <button v-if="tour.status === 1" 
                            @click.stop="archiveTour(tour)" class="btn btn-archive"><i class="fa fa-archive"></i></button>
                </template>

                <button v-if="!isAuthor" @click.stop="startTour(tour)" class="btn btn-start">
                    <i class="fa fa-play"></i> {{ getExecStatus(tour.id).btnText }}
                </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
        <div class="modal-content">
            <h3>Publish Tour</h3>
            <p>Set a price for <strong>{{ tourToPublish?.name }}</strong> to make it public.</p>
            <div class="form-group">
                <label>Price ($)</label>
                <input v-model.number="publishPrice" type="number" min="1" class="modal-input" />
            </div>
            <div class="modal-actions">
                <button @click="closeModal" class="btn-cancel">Cancel</button>
                <button @click="confirmPublish" class="btn-confirm">Publish Now</button>
            </div>
        </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useTourStore } from '@/stores/tours'
import { useAuthStore } from '@/stores/auth'
import { useExecutionStore } from '@/stores/execution'

const router = useRouter()
const tourStore = useTourStore()
const authStore = useAuthStore()
const executionStore = useExecutionStore()

const tours = ref([])
const executions = ref([]) 
const loading = ref(true)

const showModal = ref(false)
const tourToPublish = ref(null)
const publishPrice = ref(0)

const isAuthor = computed(() => {
    return authStore.user?.role === 'Author' || authStore.user?.role === 1
})

onMounted(async () => {
  try {
    if (isAuthor.value) {
        if(authStore.user?.id) tours.value = await tourStore.getToursByAuthor(authStore.user.id)
    } else {
        const [purchased, userExecs] = await Promise.all([
            tourStore.getPurchasedTours(),
            executionStore.getUserExecutions()
        ])
        tours.value = purchased
        executions.value = userExecs
    }
  } catch (err) { 
      console.error(err) 
  } finally { 
      loading.value = false 
  }
})

const getExecStatus = (tourId) => {
    if (!executions.value || executions.value.length === 0) return { label: '', class: '', isFinished: false, btnText: 'Start' }
    
    const tourSessions = executions.value
        .filter(e => e.tourId === tourId)
        .sort((a, b) => new Date(b.startedAt) - new Date(a.startedAt));

    if (tourSessions.length === 0) return { label: '', class: '', isFinished: false, btnText: 'Start' }
    
    const latest = tourSessions[0]; 

    if (latest.status === 'Completed') {
        return { label: 'Done', class: 'status-done', isFinished: true, lastDate: latest.finishedAt, btnText: 'Repeat' }
    }
    if (latest.status === 'Active') {
        return { label: 'Live', class: 'status-live', isFinished: false, btnText: 'Continue' }
    }
    return { label: 'Paused', class: 'status-redo', isFinished: false, btnText: 'Restart' }
}

const formatDate = (d) => {
    if(!d) return ''
    return new Date(d).toLocaleDateString(undefined, { month: 'short', day: 'numeric' })
}

const truncateText = (text, length) => {
  if (!text) return ''
  return text.length > length ? text.substring(0, length) + '...' : text
}

const getStatusName = (status) => {
    return status === 0 ? 'Draft' : status === 1 ? 'Published' : 'Archived'
}

const startTour = async (tour) => {
    try {
        const execId = await executionStore.startTour(tour.id)
        router.push({ name: 'active-tour', params: { id: execId }, query: { tourId: tour.id } })
    } catch (e) { 
        alert("Error starting tour: " + (e.response?.data?.message || e.message)) 
    }
}

const openPublishModal = (tour) => {
    if (!tour.name || !tour.description || !tour.keypoints || tour.keypoints.length < 2) {
         alert("Cannot publish: Tour must have basic info and at least 2 keypoints!");
         return;
    }
    tourToPublish.value = tour;
    publishPrice.value = tour.price || 0;
    showModal.value = true;
}

const closeModal = () => {
    showModal.value = false;
    tourToPublish.value = null;
}

const confirmPublish = async () => {
    if (publishPrice.value <= 0) {
        alert("Price must be greater than 0");
        return;
    }
    try {
        await tourStore.updateStatus(tourToPublish.value.id, 1, publishPrice.value);
        tourToPublish.value.status = 1;
        tourToPublish.value.price = publishPrice.value;
        closeModal();
        alert("Tour published successfully!");
    } catch (e) {
        alert("Failed to publish: " + (e.message || e));
    }
}

const archiveTour = async (tour) => {
    if (!confirm("Are you sure you want to archive this tour?")) return;
    try {
        await tourStore.updateStatus(tour.id, 2, null);
        tour.status = 2;
    } catch (e) {
        alert("Failed to archive");
    }
}
</script>

<style scoped>
.header-section { display: flex; justify-content: space-between; align-items: center; margin: 30px 0; border-bottom: 1px solid #eee; padding-bottom: 20px; }
.header-section h2 { margin: 0; color: #2c3e50; }

.tours-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 25px; }

.tour-card { background: white; cursor: pointer; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.1); transition: transform 0.2s; display: flex; flex-direction: column; }
.tour-card:hover { transform: translateY(-5px); box-shadow: 0 8px 20px rgba(0,0,0,0.15); }

.card-header { height: 80px; position: relative; background: #ddd; }

.diff-1 { background: #cc072a; } .diff-2 { background: #b00626; } .diff-3 { background: #99051f; } .diff-4 { background: #7a0419; } .diff-5 { background: #4d020f; } 
.difficulty-badge { position: absolute; top: 10px; right: 10px; background: rgba(0,0,0,0.6); color: white; padding: 4px 8px; border-radius: 12px; font-size: 0.8rem; font-weight: bold; }

.status-badge {
    position: absolute; top: 10px; left: 10px; 
    background: white; color: #333; 
    padding: 4px 10px; border-radius: 12px; 
    font-size: 0.75rem; font-weight: bold;
    box-shadow: 0 2px 5px rgba(0,0,0,0.2);
    text-transform: uppercase;
}
.status-0 { color: #7f8c8d; border: 1px solid #7f8c8d; }
.status-1 { color: #2ecc71; border: 1px solid #2ecc71; }
.status-2 { color: #f39c12; border: 1px solid #f39c12; }

.card-body { padding: 20px; display: flex; flex-direction: column; flex-grow: 1; }
.card-body h3 { font-size: 1.2rem; margin: 0 0 5px 0; color: #2c3e50; }
.tags { font-size: 0.8rem; color: #cc072a; margin-bottom: 10px; }
.tag { margin-right: 5px; }
.description { color: #666; font-size: 0.85rem; margin-bottom: 20px; flex-grow: 1; line-height: 1.4; }

.card-footer { display: flex; justify-content: space-between; align-items: center; border-top: 1px solid #eee; padding-top: 15px; margin-top: auto; }
.price { color: #2c3e50; font-size: 1.3rem; font-weight: 800; }
.price-draft { color: #999; font-style: italic; font-size: 0.9rem; }

.buttons { display: flex; gap: 8px; }

.btn-primary { background: #cc072a; color: white; padding: 10px 20px; border: none; border-radius: 6px; cursor: pointer; display: flex; align-items: center; gap: 8px; font-weight: 600; }
.btn-primary:hover { background: #99051f; }

.btn-publish { background: #2ecc71; color: white; border: none; width: 35px; height: 35px; border-radius: 50%; cursor: pointer; display: flex; align-items: center; justify-content: center; transition: 0.2s; }
.btn-publish:hover { background: #27ae60; transform: scale(1.1); }

.btn-archive { background: #f1c40f; color: white; border: none; width: 35px; height: 35px; border-radius: 50%; cursor: pointer; display: flex; align-items: center; justify-content: center; transition: 0.2s; }
.btn-archive:hover { background: #f39c12; transform: scale(1.1); }

.btn-start { background: #2ecc71; color: white; border: none; padding: 8px 16px; border-radius: 6px; cursor: pointer; font-weight: bold; display: flex; align-items: center; gap: 5px; }
.btn-start:hover { background: #27ae60; transform: scale(1.05); }

.empty-state { text-align: center; margin-top: 50px; color: #777; }
.empty-icon { font-size: 3rem; color: #ccc; margin-bottom: 20px; }

.tour-completed { filter: grayscale(0.5); opacity: 0.9; }
.tour-completed:hover { filter: grayscale(0); opacity: 1; }

.exec-badge { border: 2px solid currentColor !important; }
.status-done { color: #2ecc71 !important; border-color: #2ecc71 !important; }
.status-live { color: #cc072a !important; border-color: #cc072a !important; animation: blink 1s infinite; }
.status-redo { color: #7f8c8d !important; border-color: #7f8c8d !important; }

.last-finished { font-size: 0.75rem; color: #27ae60; font-weight: bold; margin-bottom: 10px; }

@keyframes blink { 50% { opacity: 0.3; } }

/* MODAL STYLES */
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 2000; }
.modal-content { background: white; padding: 25px; border-radius: 12px; width: 90%; max-width: 400px; }
.modal-input { width: 100%; padding: 12px; border: 1px solid #ddd; border-radius: 6px; margin-top: 10px; }
.modal-actions { display: flex; gap: 10px; justify-content: flex-end; margin-top: 25px; }
.btn-cancel { background: #eee; border: none; padding: 10px 20px; border-radius: 6px; cursor: pointer; font-weight: 600; color: #555; }
.btn-cancel:hover { background: #ddd; }
.btn-confirm { background: #cc072a; color: white; border: none; padding: 10px 20px; border-radius: 6px; cursor: pointer; font-weight: 600; display: flex; align-items: center; gap: 8px; }
.btn-confirm:hover { background: #a50522; }
</style>