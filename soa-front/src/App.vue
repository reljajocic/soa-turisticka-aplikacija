<script setup>
import Navbar from './components/Navbar.vue'
</script>

<template>
  <header>
    <Navbar />
  </header>

  <main class="main-content">
    <RouterView />
  </main>
</template>

<style>
/* Globalni stilovi */
body { margin: 0; font-family: sans-serif; background-color: #f4f4f4; }
.container { max-width: 1200px; margin: 0 auto; padding: 20px; }
.card { background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
.form-group { margin-bottom: 15px; }
.form-group label { display: block; margin-bottom: 5px; }
.form-group input, .form-group select { width: 100%; padding: 8px; box-sizing: border-box; }
.btn { padding: 10px 20px; cursor: pointer; border: none; border-radius: 4px; }
.btn-primary { background: #007bff; color: white; }
.alert-error { color: red; margin-bottom: 10px; }
.mt-20 { margin-top: 20px; }
</style>