<template>
  <div class="container">
    <div class="card" style="max-width: 400px; margin: 50px auto;">
      <h2>Register</h2>
      <div v-if="error" class="alert-error">{{ error }}</div>
      
      <form @submit.prevent="handleRegister">
        <div class="form-group">
          <label>Username</label>
          <input v-model="form.username" type="text" required />
        </div>
        <div class="form-group">
          <label>Email</label>
          <input v-model="form.email" type="email" required />
        </div>
        <div class="form-group">
          <label>Password</label>
          <input v-model="form.password" type="password" required />
        </div>
        <div class="form-group">
          <label>Role</label>
          <select v-model="form.role" required>
            <option value="0">Tourist</option>
            <option value="1">Guide/Author</option>
          </select>
        </div>
        <button type="submit" class="btn btn-primary">Register</button>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

const authStore = useAuthStore()
const router = useRouter()
const error = ref('')
const form = ref({ username: '', email: '', password: '', role: 0 })

const handleRegister = async () => {
  try {
    await authStore.register(form.value)
    router.push('/login') // Posle registracije idemo na login
  } catch (err) {
    error.value = 'Registration failed. Try changing username.'
    console.error(err)
  }
}
</script>