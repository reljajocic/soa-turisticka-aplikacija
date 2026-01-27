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
          <router-link v-if="authStore.isGuide()" to="/my-tours">My Tours</router-link>
          <router-link v-if="authStore.isTourist()" to="/my-tours">Purchased</router-link>
        </template>
      </div>

      <div class="auth-section">
        
        <template v-if="!authStore.isAuthenticated">
          <router-link to="/login" class="nav-btn login-btn">Login</router-link>
          <router-link to="/register" class="nav-btn register-btn">Register</router-link>
        </template>
        
        <template v-else>
          <router-link to="/cart" class="circle-btn cart-btn" title="Shopping Cart">
            <i class="fa fa-shopping-cart"></i>
            <span v-if="cartStore.items.length > 0" class="badge-count">{{ cartStore.items.length }}</span>
          </router-link>

          <div class="user-menu-container" ref="dropdownRef">
            
            <button @click="toggleDropdown" class="circle-btn user-avatar">
              <img v-if="userAvatarUrl" :src="userAvatarUrl" alt="User" class="nav-avatar-img" />
              <span v-else>{{ userInitials }}</span>
            </button>

            <div v-if="isDropdownOpen" class="dropdown-menu">
                <div class="dropdown-header">
                    <span class="user-name">@{{ authStore.user?.username }}</span>
                </div>
                
                <router-link to="/profile" class="dropdown-item" @click="closeDropdown">
                    <i class="fa fa-user"></i> My Profile
                </router-link>
                
                <div class="divider"></div>
                
                <button @click="handleLogout" class="dropdown-item logout-item">
                    <i class="fa fa-sign-out"></i> Logout
                </button>
            </div>

          </div>
        </template>

      </div>
    </div>
  </nav>
</template>

<script setup>
import { computed, ref, onMounted, onUnmounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useCartStore } from '@/stores/cart'
import { useRouter } from 'vue-router'
import logo from '@/assets/travela_logo.png'

const authStore = useAuthStore()
const cartStore = useCartStore()
const router = useRouter()

// Stanje za dropdown
const isDropdownOpen = ref(false)
const dropdownRef = ref(null)

// Računamo inicijale
const userInitials = computed(() => {
  return authStore.user?.username?.charAt(0).toUpperCase() || 'U'
})

// Računamo URL slike (Prilagodi polje 'avatarUrl' ili 'profileImage' onome što stiže sa backenda u authStore.user)
const userAvatarUrl = computed(() => {
    return authStore.user?.avatarUrl || authStore.user?.profileImage || null
})

// Toggle funkcija
const toggleDropdown = () => {
    isDropdownOpen.value = !isDropdownOpen.value
}

const closeDropdown = () => {
    isDropdownOpen.value = false
}

const handleLogout = () => {
  closeDropdown()
  authStore.logout()
  router.push('/login')
}

// Logika za "Click Outside" - zatvori meni ako klikneš van njega
const handleClickOutside = (event) => {
    if (dropdownRef.value && !dropdownRef.value.contains(event.target)) {
        closeDropdown()
    }
}

onMounted(() => {
    document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
    document.removeEventListener('click', handleClickOutside)
})
</script>

<style scoped>
.navbar { background-color: #ffffff; box-shadow: 0 2px 10px rgba(0,0,0,0.1); position: sticky; top: 0; z-index: 1000; padding: 0.5rem 0; }
.nav-content { display: flex; justify-content: space-between; align-items: center; }

.brand { cursor: pointer; display: flex; align-items: center; }
.logo-img { height: 40px; width: auto; object-fit: contain; }

.nav-links { display: flex; gap: 20px; }
.nav-links a { text-decoration: none; color: #666; font-weight: 500; transition: color 0.3s; font-size: 1rem; }
.nav-links a:hover, .nav-links a.router-link-active { color: #cc072a; font-weight: 700; }

.auth-section { display: flex; align-items: center; gap: 15px; }

/* Dugmići */
.nav-btn { text-decoration: none; padding: 8px 16px; border-radius: 20px; font-size: 0.9rem; transition: all 0.3s; }
.login-btn { color: #cc072a; }
.register-btn { background-color: #cc072a; color: white; }
.register-btn:hover { background-color: #cc072a; }

/* Kružići i Avatar */
.circle-btn {
  width: 40px; height: 40px; border-radius: 50%;
  display: flex; justify-content: center; align-items: center;
  text-decoration: none; font-weight: bold; font-size: 1.1rem;
  transition: transform 0.2s; position: relative; border: none; cursor: pointer;
  padding: 0;  /* Bitno za sliku */
}
.circle-btn:hover { transform: scale(1.05); }

.cart-btn { background: #f8f9fa; border: 1px solid #ddd; color: #333; }
.user-avatar { background: #cc072a; color: white; }

/* Slika u krugu */
.nav-avatar-img {
    width: 100%; height: 100%; object-fit: cover;
}

.badge-count {
    position: absolute; top: -2px; right: -2px;
    background: #cc072a; color: white;
    font-size: 0.7rem; width: 16px; height: 16px;
    border-radius: 50%; display: flex; align-items: center; justify-content: center;
    z-index: 110;
}

/* --- DROPDOWN MENI STILOVI --- */
.user-menu-container {
    position: relative; /* Bitno za pozicioniranje menija */
}

.dropdown-menu {
    position: absolute;
    top: 50px; /* Ispod avatara */
    right: 0;
    background: white;
    min-width: 180px;
    border-radius: 8px;
    box-shadow: 0 5px 15px rgba(0,0,0,0.15);
    padding: 10px 0;
    animation: fadeIn 0.2s ease-in-out;
    border: 1px solid #eee;
}

.dropdown-header {
    padding: 5px 15px 10px;
    border-bottom: 1px solid #eee;
    margin-bottom: 5px;
}
.user-name {
    font-weight: bold; color: #333; font-size: 0.9rem;
    display: block; overflow: hidden; text-overflow: ellipsis;
}

.dropdown-item {
    display: flex; align-items: center; gap: 10px;
    width: 100%; padding: 10px 15px;
    text-decoration: none; color: #555;
    background: none; border: none;
    cursor: pointer; text-align: left;
    font-size: 0.95rem; font-family: inherit;
    transition: background 0.2s;
}

.dropdown-item:hover {
    background-color: #f8f9fa;
    color: #cc072a;
}

.divider { height: 1px; background: #eee; margin: 5px 0; }

.logout-item { color: #ff4d4d; }
.logout-item:hover { background-color: #fff0f0; }

@keyframes fadeIn {
    from { opacity: 0; transform: translateY(-10px); }
    to { opacity: 1; transform: translateY(0); }
}
</style>