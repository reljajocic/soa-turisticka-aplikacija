<template>
  <div class="container">
    <div class="card form-card">
      <h2>Write a New Blog</h2>
      
      <form @submit.prevent="handleSubmit">
        <div class="form-group">
            <label>Title</label>
            <input v-model="form.title" type="text" placeholder="e.g. My Trip to Paris" required />
        </div>

        <div class="form-group">
            <label>Description (Markdown supported)</label>
            <textarea v-model="form.description" rows="8" placeholder="Share your experience..." required></textarea>
        </div>

        <div class="form-group">
            <label>Image URL (Optional)</label>
            <input v-model="imageInput" type="text" placeholder="https://..." />
            <small>Paste an image link.</small>
        </div>

        <div class="actions">
            <button type="button" @click="$router.back()" class="btn btn-secondary">Cancel</button>
            <button type="submit" class="btn btn-primary">Publish Blog</button>
        </div>
      </form>

    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useBlogStore } from '@/stores/blog'

const blogStore = useBlogStore()
const router = useRouter()

const form = ref({
    title: '',
    description: '',
    imageUrls: []
})
const imageInput = ref('')

const handleSubmit = async () => {
    if(imageInput.value) form.value.imageUrls.push(imageInput.value)
    
    try {
        await blogStore.createBlog(form.value)
        alert("Blog published successfully!")
        router.push('/blogs')
    } catch (e) {
        alert("Error: " + e)
    }
}
</script>

<style scoped>
.form-card { max-width: 700px; margin: 40px auto; padding: 30px; }
h2 { text-align: center; margin-bottom: 30px; color: #2c3e50; }

.form-group { margin-bottom: 20px; }
.form-group label { display: block; margin-bottom: 8px; font-weight: 600; color: #444; }
.form-group input, .form-group textarea {
    width: 100%; padding: 12px; border: 1px solid #ddd; border-radius: 8px;
    font-size: 1rem; font-family: inherit;
}
.form-group input:focus, .form-group textarea:focus { border-color: #cc072a; outline: none; }

.actions { display: flex; justify-content: space-between; margin-top: 30px; }
.btn { padding: 12px 25px; border-radius: 8px; font-weight: bold; cursor: pointer; border: none; }
.btn-primary { background: #cc072a; color: white; }
.btn-secondary { background: #eee; color: #333; }
</style>