<script setup>
import Navbar from './components/Navbar.vue'
import Footer from './components/Footer.vue'
import SimulatorFAB from './components/SimulatorFAB.vue' 
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore() // <--- INICIJALIZACIJA
</script>

<template>
  <div class="app-wrapper">
    <Navbar />

    <main class="main-content">
      <router-view />
    </main>

    <SimulatorFAB v-if="authStore.isTourist()" />

    <Footer />
  </div>
</template>

<style>
/* Globalni reset */
body { 
  margin: 0; 
  padding: 0; 
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
  background-color: #f8f9fa; 
  box-sizing: border-box;
  color: #2c3e50;
}

*, *:before, *:after {
  box-sizing: inherit;
}

.container { 
  max-width: 1200px; 
  margin: 0 auto; 
  padding: 20px; 
}

/* --- LAYOUT FIX --- */
.app-wrapper {
  display: flex;
  flex-direction: column;
  min-height: 100vh; /* Cela visina ekrana */
}

.main-content {
  flex: 1; /* Zauzmi sav slobodan prostor */
  width: 100%; /* OBAVEZNO: Da se sadržaj ne skuplja */
  /* IZBAČENO: display: flex; -> Ovo je pravilo problem! */
}

/* Globalni stilovi */
.btn { 
  cursor: pointer; 
  border: none; 
  border-radius: 4px; 
  padding: 10px 20px; 
  font-weight: bold; 
  transition: 0.2s; 
}
.btn-primary { background: #cc072a; color: white; }
.btn-primary:hover { background: #99051f; }

.card { 
  background: white; 
  padding: 20px; 
  border-radius: 12px; 
  box-shadow: 0 4px 15px rgba(0,0,0,0.05); 
}
</style>