<template>
  <nav class="navbar">
    <div class="container nav-content">
      <h1 @click="$router.push('/')" style="cursor: pointer;">TouristApp</h1>
      
      <div class="links">
        <router-link to="/">Home</router-link>
        
        <template v-if="!authStore.isAuthenticated">
          <router-link to="/login">Login</router-link>
          <router-link to="/register">Register</router-link>
        </template>
        
        <template v-else>
            <button @click="handleLogout" class="btn-logout">Logout</button>
        </template>
      </div>
    </div>
  </nav>
</template>

<script setup>
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

const authStore = useAuthStore()
const router = useRouter()

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}
</script>

<style scoped>
.navbar { background: #333; color: white; padding: 1rem; }
.nav-content { display: flex; justify-content: space-between; align-items: center; }
.links a { color: white; text-decoration: none; margin-left: 15px; }
.btn-logout { margin-left: 15px; background: red; color: white; border: none; padding: 5px 10px; cursor: pointer; }
</style>