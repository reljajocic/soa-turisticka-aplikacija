<template>
  <div class="container">
    
    <div class="header-section">
      <h1>Community Blog</h1>
      <button @click="$router.push('/create-blog')" class="btn-create">
        <i class="fa fa-pen"></i> Write a Story
      </button>
    </div>

    <div v-if="blogStore.loading" class="text-center">Loading stories...</div>
    
    <div v-else-if="blogStore.blogs.length === 0" class="empty-state">
      <div class="empty-icon"><i class="fa fa-feather-alt"></i></div>
      <h3>No blogs yet.</h3>
      <p>Be the first to share your adventure!</p>
    </div>

    <div v-else class="feed">
      <div v-for="blog in blogStore.blogs" :key="blog.id" class="blog-card" @click="$router.push(`/blog/${blog.id}`)">
        
        <div class="card-content">
            
            <div class="card-meta">
                <div class="author-info" @click.stop="goToProfile(blog.authorId)">
                    <img v-if="blog.authorAvatarUrl" :src="blog.authorAvatarUrl" class="tiny-avatar" />
                    <span v-else class="tiny-avatar-placeholder">{{ blog.username.charAt(0).toUpperCase() }}</span>
                    <span class="username">@{{ blog.username }}</span>
                </div>
                <span class="dot">•</span>
                <span class="date">{{ new Date(blog.createdAt).toLocaleDateString() }}</span>
            </div>

            <h2 class="card-title">{{ blog.title }}</h2>

            <p class="card-desc">
              {{ truncateText(markdownToText(blog.description), 150) }}
            </p>


            <div class="card-footer">
                <div class="chip">
                    <i class="fa fa-comment-alt"></i> 
                    {{ blog.comments ? blog.comments.length : 0 }} Comments
                </div>
            </div>
        </div>

        <div v-if="blog.imageUrls && blog.imageUrls.length" class="card-image">
            <img :src="blog.imageUrls[0]" alt="Blog thumbnail" />
        </div>

      </div>
    </div>

  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useBlogStore } from '@/stores/blog'

import MarkdownIt from 'markdown-it'
import DOMPurify from 'dompurify'

const router = useRouter()
const blogStore = useBlogStore()

const md = new MarkdownIt({
  html: false,
  linkify: true,
  breaks: true
})

onMounted(async () => {
  await blogStore.getAllBlogs()
})

const markdownToText = (markdown) => {
  if (!markdown) return ''

  const html = md.render(markdown)
  const safe = DOMPurify.sanitize(html)

  const temp = document.createElement('div')
  temp.innerHTML = safe
  return (temp.textContent || temp.innerText || '').trim()
}

const truncateText = (text, length) => {
  if (!text) return ''
  return text.length > length ? text.substring(0, length) + '...' : text
}

const goToProfile = (id) => {
  router.push(`/profile/${id}`)
}
</script>


<style scoped>
.container { max-width: 900px; margin: 0 auto; padding: 40px 20px; }

/* HEADER */
.header-section { display: flex; justify-content: space-between; align-items: center; margin-bottom: 30px; border-bottom: 1px solid #eee; padding-bottom: 20px; }
.header-section h1 { font-size: 2rem; color: #2c3e50; margin: 0; font-weight: 800; }

.btn-create { 
    background: #cc072a; color: white; border: none; padding: 10px 24px; 
    border-radius: 8px; font-weight: 600; cursor: pointer; transition: 0.2s; 
    display: flex; align-items: center; gap: 8px; font-size: 1rem;
    box-shadow: 0 4px 6px rgba(204, 7, 42, 0.2);
}
.btn-create:hover { background: #99051f; transform: translateY(-2px); }

/* FEED LAYOUT */
.feed { display: flex; flex-direction: column; gap: 20px; }

/* BLOG CARD - Travela Style */
.blog-card { 
    background: white; 
    border: none; /* Uklonjen sivi border */
    border-radius: 12px; /* Veći radius kao na Tours karticama */
    padding: 20px; 
    cursor: pointer; 
    display: flex; 
    justify-content: space-between;
    gap: 25px;
    box-shadow: 0 4px 15px rgba(0,0,0,0.05); /* Soft shadow */
    transition: transform 0.2s, box-shadow 0.2s;
}

.blog-card:hover { 
    transform: translateY(-3px); 
    box-shadow: 0 8px 25px rgba(0,0,0,0.1); 
}

/* CONTENT SIDE */
.card-content { flex: 1; display: flex; flex-direction: column; }

/* Meta info */
.card-meta { display: flex; align-items: center; gap: 8px; font-size: 0.85rem; color: #888; margin-bottom: 10px; }
.author-info { display: flex; align-items: center; gap: 8px; font-weight: 600; color: #2c3e50; transition: 0.2s; }
.author-info:hover { color: #cc072a; }

.tiny-avatar { width: 24px; height: 24px; border-radius: 50%; object-fit: cover; }
.tiny-avatar-placeholder { width: 24px; height: 24px; border-radius: 50%; background: #cc072a; color: white; display: flex; align-items: center; justify-content: center; font-size: 0.75rem; font-weight: bold; }

.card-title { font-size: 1.4rem; font-weight: 700; color: #2c3e50; margin: 0 0 10px 0; line-height: 1.3; }
.card-desc { font-size: 1rem; color: #555; margin: 0 0 15px 0; line-height: 1.6; flex-grow: 1; }

.card-footer { display: flex; align-items: center; }
.chip { 
    background: #f8f9fa; color: #666; padding: 6px 12px; 
    border-radius: 20px; font-size: 0.85rem; font-weight: 600; 
    display: flex; align-items: center; gap: 6px; transition: 0.2s;
}
.chip:hover { background: #eef1f3; color: #cc072a; }

/* IMAGE SIDE (Thumbnail) */
.card-image { width: 160px; height: 120px; flex-shrink: 0; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }
.card-image img { width: 100%; height: 100%; object-fit: cover; transition: transform 0.3s; }
.blog-card:hover .card-image img { transform: scale(1.05); }

/* EMPTY STATE */
.empty-state { text-align: center; margin-top: 50px; color: #777; padding: 40px; background: white; border-radius: 12px; box-shadow: 0 4px 15px rgba(0,0,0,0.05); }
.empty-icon { font-size: 3rem; color: #ddd; margin-bottom: 15px; }

/* RESPONSIVE */
@media (max-width: 650px) {
    .blog-card { flex-direction: column-reverse; }
    .card-image { width: 100%; height: 180px; margin-bottom: 15px; }
}
</style>