<template>
  <div class="container">
    <div v-if="loading" class="text-center">Loading profile...</div>
    <div v-else-if="error" class="text-center error">{{ error }}</div>
    
    <div v-else class="profile-card">
        <div class="profile-header">
            <div class="header-bg"></div>
            <div class="avatar-wrapper">
                <img v-if="profile.avatarUrl" :src="profile.avatarUrl" class="avatar" />
                <div v-else class="avatar-placeholder">
                    {{ profile.username?.charAt(0).toUpperCase() }}
                </div>
            </div>
        </div>

        <div class="profile-body">
            <h1 class="username">@{{ profile.username }}</h1>
            <p class="role-badge">{{ profile.role }}</p>
            
            <div v-if="authStore.user?.id !== profile.id" class="follow-section">
                <button 
                    @click="toggleFollow" 
                    class="btn" 
                    :class="isFollowing ? 'btn-unfollow' : 'btn-follow'"
                    :disabled="actionLoading"
                >
                    <i class="fa" :class="isFollowing ? 'fa-user-times' : 'fa-user-plus'"></i>
                    {{ isFollowing ? 'Unfollow' : 'Follow' }}
                </button>
            </div>

            <div class="bio-section">
                <p v-if="profile.firstName || profile.lastName" class="full-name">
                    {{ profile.firstName }} {{ profile.lastName }}
                </p>
                <p v-if="profile.motto" class="motto">"{{ profile.motto }}"</p>
                <hr class="divider" />
                <h3>About</h3>
                <p class="bio">{{ profile.bio || 'This user has not written a biography yet.' }}</p>
            </div>
            
            <button @click="$router.back()" class="btn-back">
                <i class="fa fa-arrow-left"></i> Go Back
            </button>
        </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { useProfileStore } from '@/stores/profile'
import { useAuthStore } from '@/stores/auth'
import { useFollowStore } from '@/stores/follow' // <--- NOVO

const route = useRoute()
const profileStore = useProfileStore()
const authStore = useAuthStore()
const followStore = useFollowStore() // <--- NOVO

const profile = ref({})
const loading = ref(true)
const error = ref('')
const isFollowing = ref(false)     // <--- Pratimo status
const actionLoading = ref(false)   // <--- Da sprečimo spam kliktanje

onMounted(async () => {
    try {
        const userId = route.params.id
        profile.value = await profileStore.getPublicProfile(userId)
        
        // Proverimo da li ga već pratimo (ako smo ulogovani i nije naš profil)
        if (authStore.isAuthenticated && authStore.user.id !== userId) {
            isFollowing.value = await followStore.isFollowing(userId)
        }
    } catch (e) {
        error.value = "User not found."
    } finally {
        loading.value = false
    }
})

const toggleFollow = async () => {
    if (!authStore.isAuthenticated) {
        alert("Please login to follow users.")
        return
    }
    
    actionLoading.value = true
    try {
        if (isFollowing.value) {
            await followStore.unfollowUser(profile.value.id)
            isFollowing.value = false
        } else {
            await followStore.followUser(profile.value.id)
            isFollowing.value = true
        }
    } catch (e) {
        alert("Action failed.")
    } finally {
        actionLoading.value = false
    }
}
</script>

<style scoped>
.container { display: flex; justify-content: center; padding-top: 40px; }
.profile-card { 
    background: white; width: 100%; max-width: 600px; 
    border-radius: 16px; overflow: hidden; 
    box-shadow: 0 10px 30px rgba(0,0,0,0.1); 
    text-align: center;
}

.profile-header { position: relative; margin-bottom: 60px; }
.header-bg { height: 140px; background: linear-gradient(135deg, #cc072a, #ff4d6d); }
.avatar-wrapper {
    position: absolute; bottom: -50px; left: 50%; transform: translateX(-50%);
    width: 120px; height: 120px; border-radius: 50%; 
    border: 5px solid white; background: white;
    display: flex; justify-content: center; align-items: center;
    box-shadow: 0 5px 15px rgba(0,0,0,0.1);
    overflow: hidden; /* BITNO DA BUDE KRUG */
}
.avatar { width: 100%; height: 100%; object-fit: cover; }
.avatar-placeholder { font-size: 3rem; font-weight: bold; color: #cc072a; }

.profile-body { padding: 0 30px 40px; }
.username { margin: 0; color: #333; font-size: 1.8rem; }
.role-badge { 
    display: inline-block; background: #eee; color: #555; 
    padding: 4px 12px; border-radius: 20px; font-size: 0.8rem; 
    margin-top: 5px; font-weight: 600; text-transform: uppercase;
}

.bio-section { margin-top: 30px; }
.full-name { font-size: 1.2rem; font-weight: bold; color: #cc072a; margin-bottom: 5px; }
.motto { font-style: italic; color: #777; margin-bottom: 20px; }
.divider { border: 0; border-top: 1px solid #eee; margin: 20px 0; }
.bio { color: #555; line-height: 1.6; }

.btn-back { margin-top: 30px; background: white; border: 1px solid #ddd; padding: 10px 20px; border-radius: 8px; cursor: pointer; transition: 0.2s; }
.btn-back:hover { background: #f9f9f9; border-color: #bbb; }
.container { display: flex; justify-content: center; padding-top: 40px; }
.profile-card { background: white; width: 100%; max-width: 600px; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 30px rgba(0,0,0,0.1); text-align: center; }
.profile-header { position: relative; margin-bottom: 60px; }
.header-bg { height: 140px; background: linear-gradient(135deg, #cc072a, #ff4d6d); }
.avatar-wrapper { position: absolute; bottom: -50px; left: 50%; transform: translateX(-50%); width: 120px; height: 120px; border-radius: 50%; border: 5px solid white; background: white; display: flex; justify-content: center; align-items: center; box-shadow: 0 5px 15px rgba(0,0,0,0.1); overflow: hidden; }
.avatar { width: 100%; height: 100%; object-fit: cover; }
.avatar-placeholder { font-size: 3rem; font-weight: bold; color: #cc072a; }
.profile-body { padding: 0 30px 40px; }
.username { margin: 0; color: #333; font-size: 1.8rem; }
.role-badge { display: inline-block; background: #eee; color: #555; padding: 4px 12px; border-radius: 20px; font-size: 0.8rem; margin-top: 5px; font-weight: 600; text-transform: uppercase; }
.bio-section { margin-top: 30px; }
.full-name { font-size: 1.2rem; font-weight: bold; color: #cc072a; margin-bottom: 5px; }
.motto { font-style: italic; color: #777; margin-bottom: 20px; }
.divider { border: 0; border-top: 1px solid #eee; margin: 20px 0; }
.bio { color: #555; line-height: 1.6; }
.btn-back { margin-top: 30px; background: white; border: 1px solid #ddd; padding: 10px 20px; border-radius: 8px; cursor: pointer; transition: 0.2s; }
.btn-back:hover { background: #f9f9f9; border-color: #bbb; }

/* NOVI STILOVI ZA FOLLOW DUGME */
.follow-section { margin-top: 20px; }
.btn { padding: 10px 24px; border-radius: 25px; cursor: pointer; font-weight: bold; border: none; font-size: 1rem; transition: 0.2s; display: inline-flex; align-items: center; gap: 8px; }
.btn-follow { background-color: #cc072a; color: white; }
.btn-follow:hover { background-color: #a50522; transform: scale(1.05); }
.btn-unfollow { background-color: #eee; color: #333; border: 1px solid #ccc; }
.btn-unfollow:hover { background-color: #ddd; color: #cc072a; }
.btn:disabled { opacity: 0.6; cursor: not-allowed; }
</style>