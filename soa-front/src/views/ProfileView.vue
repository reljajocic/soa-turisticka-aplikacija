<template>
  <div class="container">
    <div class="profile-wrapper">
      
      <div v-if="loading" class="text-center">
        <div class="spinner"></div>
        <p>Loading profile...</p>
      </div>

      <div v-else-if="error" class="error-box">
        <i class="fa fa-exclamation-circle"></i> {{ error }}
      </div>

      <div v-else class="card profile-card">
        
        <div class="profile-header">
          <div class="header-bg"></div>
          
          <div class="avatar-container">
            <img v-if="form.avatarUrl" :src="form.avatarUrl" alt="Avatar" class="avatar-img" @error="imageLoadError">
            
            <div v-else class="avatar-placeholder">
              {{ profileStore.profile?.username?.charAt(0).toUpperCase() }}
            </div>
            
            <span class="role-badge">
              {{ profileStore.profile?.role || 'User' }}
            </span>
          </div>

          <h2 class="username">@{{ profileStore.profile?.username }}</h2>
          <p class="email">{{ profileStore.profile?.email }}</p>
        </div>

        <form @submit.prevent="handleUpdate" class="profile-form">
            
            <div class="form-row">
                <div class="form-group">
                    <label>First Name</label>
                    <input v-model="form.firstName" type="text" placeholder="e.g. Marko" />
                </div>
                
                <div class="form-group">
                    <label>Last Name</label>
                    <input v-model="form.lastName" type="text" placeholder="e.g. Markovic" />
                </div>
            </div>

            <div class="form-group">
                <label>Profile Image URL</label>
                <input v-model="form.avatarUrl" type="text" placeholder="https://..." />
                <small class="hint">Paste an image link from the internet.</small>
            </div>

            <div class="form-group">
                <label>Motto</label>
                <input v-model="form.motto" type="text" placeholder="Your life motto..." />
            </div>

            <div class="form-group">
                <label>Biography</label>
                <textarea v-model="form.bio" rows="4" placeholder="Tell us something about yourself..."></textarea>
            </div>

            <div class="form-actions">
                <button type="submit" class="btn btn-primary btn-save">
                    <i class="fa fa-save"></i> Save Changes
                </button>
            </div>

            <div v-if="successMsg" class="success-msg">
                <i class="fa fa-check-circle"></i> {{ successMsg }}
            </div>
        </form>

      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref, reactive } from 'vue'
import { useProfileStore } from '@/stores/profile'
import { useAuthStore } from '@/stores/auth' // <--- IMPORT AUTH STORE

const profileStore = useProfileStore()
const authStore = useAuthStore() // <--- INIT
const loading = ref(true)
const error = ref('')
const successMsg = ref('')

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
    
    Object.assign(form, {
        firstName: data.firstName || '',
        lastName: data.lastName || '',
        bio: data.bio || '',
        motto: data.motto || '',
        avatarUrl: data.avatarUrl || '' 
    })
    
    if(data.avatarUrl) {
        authStore.updateUser({ avatarUrl: data.avatarUrl })
    }

  } catch (err) {
    error.value = 'Failed to load profile data.'
    console.error(err)
  } finally {
    loading.value = false
  }
})

const handleUpdate = async () => {
    successMsg.value = ''
    try {
        await profileStore.updateProfile(form)
        await profileStore.getMyProfile()
        
        authStore.updateUser({
            username: form.username, 
            avatarUrl: form.avatarUrl
        })

        successMsg.value = 'Profile updated successfully!'
        setTimeout(() => successMsg.value = '', 3000)
    } catch (err) {
        alert('Update failed: ' + (err.response?.data || err.message))
    }
}

const imageLoadError = () => {
    form.avatarUrl = '' 
}
</script>

<style scoped>
/* Tvoji stilovi ostaju isti */
.profile-wrapper { display: flex; justify-content: center; padding-top: 30px; padding-bottom: 50px; }
.profile-card { width: 100%; max-width: 700px; padding: 0; overflow: hidden; border: none; box-shadow: 0 10px 30px rgba(0,0,0,0.08); background: white; }
.profile-header { background-color: white; text-align: center; padding-bottom: 30px; border-bottom: 1px solid #eee; position: relative; }
.header-bg { height: 120px; background: linear-gradient(135deg, #cc072a, #ff4d6d); width: 100%; }
.avatar-container { position: relative; width: 120px; height: 120px; margin: -60px auto 15px; }
.avatar-img { width: 100%; height: 100%; border-radius: 50%; object-fit: cover; border: 5px solid white; box-shadow: 0 4px 10px rgba(0,0,0,0.1); background: white; }
.avatar-placeholder { width: 100%; height: 100%; background-color: #2c3e50; color: white; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 3rem; font-weight: bold; border: 5px solid white; box-shadow: 0 4px 10px rgba(0,0,0,0.1); }
.role-badge { position: absolute; bottom: 5px; right: 0; background: #cc072a; color: white; font-size: 0.75rem; padding: 4px 10px; border-radius: 20px; font-weight: bold; border: 2px solid white; text-transform: uppercase; }
.username { margin: 0; color: #333; font-size: 1.5rem; }
.email { margin: 5px 0 0; color: #777; font-size: 0.95rem; }
.profile-form { padding: 30px 40px; }
.form-row { display: flex; gap: 20px; }
.form-row .form-group { flex: 1; }
.form-group { margin-bottom: 20px; }
.form-group label { display: block; margin-bottom: 8px; font-weight: 600; color: #444; font-size: 0.9rem; }
.form-group input, .form-group textarea { width: 100%; padding: 12px; border: 1px solid #ddd; border-radius: 8px; font-size: 1rem; transition: 0.2s; font-family: inherit; box-sizing: border-box; }
.form-group input:focus, .form-group textarea:focus { border-color: #cc072a; outline: none; box-shadow: 0 0 0 3px rgba(204, 7, 42, 0.1); }
.hint { font-size: 0.8rem; color: #888; margin-top: 5px; display: block; }
.btn-save { width: 100%; padding: 14px; font-size: 1.1rem; display: flex; justify-content: center; align-items: center; gap: 10px; background-color: #cc072a; color: white; transition: 0.2s; cursor: pointer; border: none; border-radius: 5px; }
.btn-save:hover { background-color: #99051f; }
.success-msg { margin-top: 20px; background: #d4edda; color: #155724; padding: 15px; border-radius: 8px; text-align: center; font-weight: 500; }
.error-box { background: #f8d7da; color: #721c24; padding: 20px; border-radius: 8px; text-align: center; }
@media (max-width: 600px) { .form-row { flex-direction: column; gap: 0; } .profile-form { padding: 20px; } }
</style>