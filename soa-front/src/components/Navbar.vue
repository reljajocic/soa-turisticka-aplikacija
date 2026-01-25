<template>
  <nav class="navbar">
    <div class="container nav-content">
      <div class="brand" @click="$router.push('/')">
        <span class="logo-text">Vistara</span> 
      </div>
      
      <div class="nav-links">
        <router-link to="/">Home</router-link>
        
        <template v-if="authStore.isAuthenticated">
          <router-link to="/tours">Tours</router-link>
          <router-link to="/blogs">Blogs</router-link>
          
          <router-link v-if="authStore.isGuide()" to="/my-tours">My Tours</router-link>
        </template>
      </div>

      <div class="auth-section">
        <template v-if="!authStore.isAuthenticated">
          <router-link to="/login" class="nav-btn login-btn">Login</router-link>
          <router-link to="/register" class="nav-btn register-btn">Register</router-link>
        </template>
        
        <template v-else>
          <router-link to="/profile" class="user-profile">
            <div class="avatar-circle">
              {{ userInitials }}
            </div>
            <!-- <span class="username">{{ authStore.user?.username }}</span> -->
          </router-link>

          <button @click="handleLogout" class="logout-btn">
            Logout
          </button>
        </template>
      </div>
    </div>
  </nav>
</template>

<script setup>
import { computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

const authStore = useAuthStore()
const router = useRouter()

// Računa inicijale za kružić (npr. Relja Jocic -> R)
const userInitials = computed(() => {
  return authStore.user?.username?.charAt(0).toUpperCase() || 'P'
})

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}
</script>

<style scoped>
/* Osnovni stil navbara */
.navbar {
  background-color: #ffffff;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
  position: sticky;
  top: 0;
  z-index: 1000;
  padding: 0.8rem 0;
}

.nav-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

/* Brend / Logo */
.brand {
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 10px;
}

.logo-text {
  font-size: 1.5rem;
  font-weight: 700;
  color: #2c3e50; /* Tamno siva/plava */
  letter-spacing: -0.5px;
}

/* Linkovi u sredini */
.nav-links {
  display: flex;
  gap: 20px;
}

.nav-links a {
  text-decoration: none;
  color: #666;
  font-weight: 500;
  transition: color 0.3s;
  font-size: 1rem;
}

.nav-links a:hover {
  color: #007bff; /* Tvoja glavna boja */
}

/* Vue Router automatski dodaje ovu klasu aktivnom linku */
.nav-links a.router-link-active {
  color: #007bff;
  font-weight: 700;
}

/* Desni deo (Auth) */
.auth-section {
  display: flex;
  align-items: center;
  gap: 15px;
}

/* Dugmići za Login/Register */
.nav-btn {
  text-decoration: none;
  padding: 8px 16px;
  border-radius: 20px;
  font-size: 0.9rem;
  transition: all 0.3s;
}

.login-btn {
  color: #2c3e50;
}

.register-btn {
  background-color: #007bff;
  color: white;
}

.register-btn:hover {
  background-color: #0056b3;
}

/* Profil Avatar */
.user-profile {
  display: flex;
  align-items: center;
  gap: 8px;
  text-decoration: none;
  color: #2c3e50;
  cursor: pointer;
}

.avatar-circle {
  width: 35px;
  height: 35px;
  background: #007bff;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  font-size: 1.1rem;
}

.username {
  font-weight: 600;
  font-size: 0.95rem;
}

/* Logout dugme */
.logout-btn {
  background: none;
  border: 1px solid #ff4d4d;
  color: #ff4d4d;
  padding: 5px 12px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.8rem;
  transition: all 0.2s;
}

.logout-btn:hover {
  background: #ff4d4d;
  color: white;
}
</style>