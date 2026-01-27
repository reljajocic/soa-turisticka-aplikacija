<template>
  <div class="fab-container">
    <span class="fab-tooltip">Open Simulator</span>
    
    <button @click="toggleSimulator" class="fab-btn" :class="{ 'active': isActive }">
      <i class="fa fa-map-marker-alt"></i>
    </button>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'

const router = useRouter()
const route = useRoute()

const isActive = computed(() => route.path === '/simulator')

const toggleSimulator = () => {
    if (isActive.value) {
        router.back()
    } else {
        router.push('/simulator')
    }
}
</script>

<style scoped>
.fab-container {
    position: fixed;
    /* PODIGNUTO GORE da ne gazi copyright tekst */
    bottom: 90px; 
    right: 30px;
    z-index: 10000; /* Maksimalan z-index */
    display: flex;
    align-items: center;
    gap: 10px;
}

.fab-btn {
    width: 60px;
    height: 60px;
    border-radius: 50%;
    background-color: #cc072a; 
    color: white;
    
    /* DODAT BELI OKVIR DA SE VIDI NA CRVENOJ POZADINI */
    border: 3px solid white; 
    
    /* JAČA SENKA */
    box-shadow: 0 5px 20px rgba(0,0,0,0.3); 
    
    cursor: pointer;
    font-size: 1.5rem;
    transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
    display: flex;
    align-items: center;
    justify-content: center;
}

.fab-btn:hover {
    transform: scale(1.1);
    background-color: #a80622;
    border-color: #f0f0f0;
}

.fab-btn.active {
    background-color: #2c3e50;
    transform: rotate(180deg);
}

.fab-tooltip {
    background: #333;
    color: white;
    padding: 6px 12px;
    border-radius: 6px;
    font-size: 0.85rem;
    font-weight: bold;
    opacity: 0;
    transform: translateX(10px);
    transition: 0.3s;
    pointer-events: none;
    white-space: nowrap;
    box-shadow: 0 2px 8px rgba(0,0,0,0.2);
}

.fab-container:hover .fab-tooltip {
    opacity: 1;
    transform: translateX(0);
}
</style>