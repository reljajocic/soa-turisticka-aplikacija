<template>
  <nav class="navbar">
    <div class="container nav-content">
      <div class="brand" @click="$router.push('/')">
        <img :src="logo" alt="Travela" class="logo-img" />
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
          <router-link to="/cart" class="circle-btn cart-btn" title="Shopping Cart">
            <i class="fa fa-shopping-cart" aria-hidden="true"></i>
            <span v-if="cartStore.items.length > 0" class="badge-count">{{ cartStore.items.length }}</span>
          </router-link>

          <router-link to="/profile" class="circle-btn user-avatar">
            {{ userInitials }}
          </router-link>

          <button @click="handleLogout" class="logout-link">
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
import { useCartStore } from '@/stores/cart'
import { useRouter } from 'vue-router'

// --- IMPORTUJEMO SLIKU ---
import logo from '@/assets/travela_logo.png'

const authStore = useAuthStore()
const cartStore = useCartStore()
const router = useRouter()

const userInitials = computed(() => {
  return authStore.user?.username?.charAt(0).toUpperCase() || 'U'
})

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}
</script>

<style scoped>
.navbar { background-color: #ffffff; box-shadow: 0 2px 10px rgba(0,0,0,0.1); position: sticky; top: 0; z-index: 1000; padding: 0.5rem 0; }
.nav-content { display: flex; justify-content: space-between; align-items: center; }

.brand { cursor: pointer; display: flex; align-items: center; }

/* STIL ZA LOGO SLIKU */
.logo-img {
    height: 40px; /* Prilagodi visinu da stane u navbar */
    width: auto;   /* Širina se prilagođava automatski */
    object-fit: contain;
}

.nav-links { display: flex; gap: 20px; }
.nav-links a { text-decoration: none; color: #666; font-weight: 500; transition: color 0.3s; font-size: 1rem; }
.nav-links a:hover, .nav-links a.router-link-active { color: #cc072a; font-weight: 700; }

.auth-section { display: flex; align-items: center; gap: 15px; }

/* DUGMIĆI LOGIN/REGISTER */
.nav-btn { text-decoration: none; padding: 8px 16px; border-radius: 20px; font-size: 0.9rem; transition: all 0.3s; }
.login-btn { color: #cc072a; }
.register-btn { background-color: #cc072a; color: white; }
.register-btn:hover { background-color: #cc072a; }

/* STILOVI ZA KRUŽIĆE */
.circle-btn {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  justify-content: center;
  align-items: center;
  text-decoration: none;
  font-weight: bold;
  font-size: 1.1rem;
  transition: transform 0.2s;
  position: relative;
}
.circle-btn:hover { transform: scale(1.05); }

.cart-btn { background: #f8f9fa; border: 1px solid #ddd; color: #333; }
.badge-count {
    position: absolute; top: -2px; right: -2px;
    background: #cc072a; color: white;
    font-size: 0.7rem; width: 16px; height: 16px;
    border-radius: 50%; display: flex; align-items: center; justify-content: center;
}

.user-avatar { background: #cc072a; color: white; }

.logout-link { background: none; border: none; color: #ff4d4d; cursor: pointer; font-size: 0.9rem; }
.logout-link:hover { text-decoration: underline; }
</style>