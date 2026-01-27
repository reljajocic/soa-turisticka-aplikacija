<template>
  <div class="auth-container">
    <div class="card auth-card">
      
      <div class="auth-header">
        <h2>Welcome Back</h2>
        <p>Login to continue your journey</p>
      </div>

      <div v-if="error" class="error-box">
        <i class="fa fa-exclamation-circle"></i> {{ error }}
      </div>
      
      <form @submit.prevent="handleLogin" class="auth-form">
        <div class="form-group">
          <label>Username</label>
          <div class="input-wrapper">
            <i class="fa fa-user input-icon"></i>
            <input v-model="username" type="text" placeholder="Enter your username" required />
          </div>
        </div>

        <div class="form-group">
          <label>Password</label>
          <div class="input-wrapper">
            <i class="fa fa-lock input-icon"></i>
            <input v-model="password" type="password" placeholder="Enter your password" required />
          </div>
        </div>

        <button type="submit" class="btn btn-primary btn-block">
          Login <i class="fa fa-sign-in"></i>
        </button>
      </form>

      <div class="auth-footer">
        <p>Don't have an account? <router-link to="/register">Register here</router-link></p>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

const authStore = useAuthStore()
const router = useRouter()
const username = ref('')
const password = ref('')
const error = ref('')

const handleLogin = async () => {
  try {
    await authStore.login({ username: username.value, password: password.value })
    router.push('/')
  } catch (err) {
    error.value = 'Invalid username or password.'
  }
}
</script>

<style scoped>
/* GLOBAL AUTH STYLES (Možeš ovo staviti i u globalni css ako želiš) */
.auth-container {
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 80vh; /* Da bude centrirano vertikalno */
    padding: 20px;
}

.auth-card {
    width: 100%;
    max-width: 450px;
    padding: 40px;
    border-radius: 16px;
    box-shadow: 0 15px 35px rgba(0,0,0,0.1);
    background: white;
    border: none;
}

/* HEADER */
.auth-header { text-align: center; margin-bottom: 30px; }
.auth-header h2 { font-size: 2rem; color: #2c3e50; margin: 0 0 10px; font-weight: 800; }
.auth-header p { color: #888; font-size: 1rem; margin: 0; }

/* INPUTS */
.form-group { margin-bottom: 20px; }
.form-group label { display: block; margin-bottom: 8px; font-weight: 600; color: #444; font-size: 0.9rem; }

.input-wrapper { position: relative; }
.input-icon { position: absolute; left: 15px; top: 50%; transform: translateY(-50%); color: #999; font-size: 1.1rem;}

.form-group input {
    width: 100%;
    padding: 14px 14px 14px 45px; /* Padding levo zbog ikonice */
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 1rem;
    transition: all 0.3s;
    background-color: #f9f9f9;
}

.form-group input:focus {
    border-color: #cc072a;
    background-color: white;
    box-shadow: 0 0 0 4px rgba(204, 7, 42, 0.1);
    outline: none;
}

/* BUTTON */
.btn-block { width: 100%; padding: 14px; font-size: 1.1rem; border-radius: 8px; display: flex; justify-content: center; align-items: center; gap: 10px; transition: 0.2s; }
.btn-block:hover { transform: translateY(-2px); box-shadow: 0 5px 15px rgba(204, 7, 42, 0.3); }

/* ERROR */
.error-box { background: #fee2e2; color: #b91c1c; padding: 12px; border-radius: 8px; margin-bottom: 20px; text-align: center; font-size: 0.9rem; display: flex; align-items: center; justify-content: center; gap: 8px; }

/* FOOTER */
.auth-footer { text-align: center; margin-top: 25px; font-size: 0.95rem; color: #666; }
.auth-footer a { color: #cc072a; font-weight: bold; text-decoration: none; }
.auth-footer a:hover { text-decoration: underline; }
</style>