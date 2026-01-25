<template>
  <div class="container">
    <div class="card profile-card">
      <h2>My Profile</h2>
      
      <div v-if="loading">Loading...</div>
      <div v-else-if="error" class="alert-error">{{ error }}</div>
      
      <div v-else class="profile-content">
        <div class="header">
          <div class="avatar">
            {{ profileStore.profile?.username?.charAt(0).toUpperCase() }}
          </div>
          <h3>{{ profileStore.profile?.username }}</h3>
          <span class="badge">{{ profileStore.profile?.role === 1 ? 'Guide' : 'Tourist' }}</span>
        </div>

        <form @submit.prevent="handleUpdate" class="mt-20">
            <div class="form-group">
                <label>First Name</label>
                <input v-model="form.firstName" type="text" placeholder="Enter first name" />
            </div>
            
            <div class="form-group">
                <label>Last Name</label>
                <input v-model="form.lastName" type="text" placeholder="Enter last name" />
            </div>

            <div class="form-group">
                <label>Bio</label>
                <textarea v-model="form.bio" rows="3" placeholder="Tell us about yourself"></textarea>
            </div>

            <button type="submit" class="btn btn-primary">Save Changes</button>
            <div v-if="successMsg" class="success-msg">{{ successMsg }}</div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref, reactive } from 'vue'
import { useProfileStore } from '@/stores/profile'

const profileStore = useProfileStore()
const loading = ref(true)
const error = ref('')
const successMsg = ref('')

// Lokalna forma
const form = reactive({
    firstName: '',
    lastName: '',
    bio: '',
    motto: '',
    avatarUrl: ''
})

onMounted(async () => {
  try {
    const data = await profileStore.getMyProfile()
    // Popuni formu podacima sa servera
    Object.assign(form, {
        firstName: data.person?.name || '',
        lastName: data.person?.surname || '',
        bio: data.bio || '',
        motto: data.motto || ''
    })
  } catch (err) {
    error.value = 'Failed to load profile.'
  } finally {
    loading.value = false
  }
})

const handleUpdate = async () => {
    successMsg.value = ''
    try {
        await profileStore.updateProfile(form)
        successMsg.value = 'Profile updated successfully!'
    } catch (err) {
        error.value = 'Update failed.'
    }
}
</script>

<style scoped>
.profile-card { max-width: 600px; margin: 50px auto; }
.avatar { 
    width: 80px; height: 80px; background: #007bff; color: white; 
    border-radius: 50%; display: flex; align-items: center; justify-content: center;
    font-size: 2rem; margin: 0 auto 10px;
}
.header { text-align: center; border-bottom: 1px solid #eee; padding-bottom: 20px; }
.badge { background: #eee; padding: 2px 8px; border-radius: 12px; font-size: 0.8rem; }
.success-msg { color: green; margin-top: 10px; text-align: center; }
textarea { width: 100%; padding: 8px; }
</style>