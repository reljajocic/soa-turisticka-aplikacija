<template>
  <div class="container">
    
    <div class="header-section">
      <h2>Community Blogs</h2>
      <button v-if="authStore.isAuthenticated" @click="$router.push('/create-blog')" class="btn btn-primary">
        <i class="fa fa-pen"></i> Write a Blog
      </button>
    </div>

    <div v-if="blogStore.loading" class="text-center">Loading stories...</div>

    <div v-else class="blogs-grid">
      <div v-for="blog in blogStore.blogs" :key="blog.id" class="blog-card" @click="$router.push(`/blog/${blog.id}`)">
        
        <div class="card-header">
            <div class="author-avatar">
                {{ blog.username.charAt(0).toUpperCase() }}
            </div>
            <div class="author-info">
                <span class="author-name">@{{ blog.username }}</span>
                <span class="post-date">{{ new Date(blog.createdAt).toLocaleDateString() }}</span>
            </div>
        </div>

        <div class="card-body">
            <h3>{{ blog.title }}</h3>
            <p class="description">{{ truncateText(blog.description, 120) }}</p>
            
            <div class="meta-footer">
                <span class="comments-badge">
                    <i class="fa fa-comment"></i> {{ blog.comments ? blog.comments.length : 0 }} Comments
                </span>
            </div>
        </div>

      </div>
    </div>
    
    <div v-if="!blogStore.loading && blogStore.blogs.length === 0" class="empty-state">
      <p>No blogs yet. Be the first to write one!</p>
    </div>

  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useBlogStore } from '@/stores/blog'
import { useAuthStore } from '@/stores/auth'

const blogStore = useBlogStore()
const authStore = useAuthStore()

onMounted(async () => {
    await blogStore.getAllBlogs()
})

const truncateText = (text, length) => {
  if (!text) return ''
  return text.length > length ? text.substring(0, length) + '...' : text
}
</script>

<style scoped>
.header-section {
    display: flex; justify-content: space-between; align-items: center;
    margin: 30px 0; border-bottom: 1px solid #eee; padding-bottom: 20px;
}
.header-section h2 { margin: 0; color: #2c3e50; }

.blogs-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 25px; }

.blog-card {
    background: white; border-radius: 12px; overflow: hidden;
    box-shadow: 0 4px 15px rgba(0,0,0,0.05); transition: transform 0.2s;
    border: 1px solid #eee;
    cursor: pointer;
}
.blog-card:hover { transform: translateY(-5px); box-shadow: 0 8px 20px rgba(0,0,0,0.1); }

/* HEADER SA AUTOROM */
.card-header {
    display: flex; align-items: center; gap: 10px; padding: 15px;
    background: #f9f9f9; border-bottom: 1px solid #eee;
}
.author-avatar {
    width: 35px; height: 35px; background: #cc072a; color: white;
    border-radius: 50%; display: flex; align-items: center; justify-content: center;
    font-weight: bold; font-size: 0.9rem;
}
.author-info { display: flex; flex-direction: column; }
.author-name { font-weight: 600; font-size: 0.9rem; color: #333; }
.post-date { font-size: 0.75rem; color: #888; }

/* BODY */
.card-body { padding: 20px; }
.card-body h3 { margin: 0 0 10px 0; font-size: 1.2rem; color: #2c3e50; }
.description { color: #555; font-size: 0.95rem; line-height: 1.5; margin-bottom: 20px; }

.meta-footer { border-top: 1px solid #eee; padding-top: 15px; display: flex; justify-content: flex-end; }
.comments-badge { font-size: 0.85rem; color: #777; display: flex; align-items: center; gap: 5px; }

.btn-primary { 
    background: #cc072a; color: white; border: none; padding: 10px 20px; 
    border-radius: 20px; cursor: pointer; font-weight: 600; 
    display: flex; align-items: center; gap: 8px;
}
.btn-primary:hover { background: #99051f; }
</style>