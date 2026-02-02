<template>
  <div class="container">
    <div v-if="blogStore.loading" class="text-center">Loading story...</div>
    <div v-else-if="!blog" class="text-center">Blog post not found.</div>

    <div v-else class="blog-details">
      <button @click="$router.push('/blogs')" class="btn-back">
        <i class="fa fa-arrow-left"></i> Back to Feed
      </button>

      <div v-if="blog.imageUrls && blog.imageUrls.length" class="hero-image">
        <img :src="blog.imageUrls[0]" alt="Blog Cover" />
      </div>

      <div class="content-wrapper">
        <h1 class="title">{{ blog.title }}</h1>
        
        <div class="meta-info">
            <div class="author-section" @click="goToProfile(blog.authorId)">
                <div class="avatar">
                    <img v-if="blog.authorAvatarUrl" :src="blog.authorAvatarUrl" class="avatar-img" />
                    <span v-else>{{ blog.username.charAt(0).toUpperCase() }}</span>
                </div>
                <div class="author-text">
                    <span class="author-name">@{{ blog.username }}</span>
                    <span v-if="isFollowingAuthor" class="following-badge">Following</span>
                </div>
            </div>
            <span class="date">{{ new Date(blog.createdAt).toLocaleDateString() }}</span>
        </div>

        <hr />
        <div class="blog-body">
            <p v-for="(paragraph, index) in formattedBody" :key="index">{{ paragraph }}</p>
        </div>
        <hr />

        <div class="comments-section">
            <h3>Comments ({{ blog.comments ? blog.comments.length : 0 }})</h3>

            <div v-if="canComment" class="comment-form">
                <textarea v-model="newComment" placeholder="Write a comment..." rows="3"></textarea>
                <button @click="postComment" class="btn btn-primary" :disabled="!newComment.trim()">
                    Post Comment
                </button>
            </div>

            <div v-else-if="authStore.isAuthenticated" class="locked-comments">
                <i class="fa fa-lock"></i>
                <p>You must follow <strong>@{{ blog.username }}</strong> to leave a comment.</p>
                <button @click="goToProfile(blog.authorId)" class="btn btn-outline">
                    Go to Profile
                </button>
            </div>

            <div v-else class="login-msg">
                <router-link to="/login">Login</router-link> to follow and comment.
            </div>

            <div class="comments-list">
                <div v-for="comment in blog.comments" :key="comment.createdAt" class="comment">
                    <div class="comment-header">
                        <div class="comment-author" @click="goToProfile(comment.userId)">
                             <div class="small-avatar">
                                <img v-if="comment.authorAvatarUrl" :src="comment.authorAvatarUrl" />
                                <span v-else>{{ comment.username.charAt(0).toUpperCase() }}</span>
                             </div>
                             <strong>@{{ comment.username }}</strong>
                        </div>
                        <span class="comment-date">{{ new Date(comment.createdAt).toLocaleDateString() }}</span>
                    </div>
                    <p>{{ comment.content }}</p>
                </div>
                <p v-if="!blog.comments || blog.comments.length === 0" class="no-comments">
                    No comments yet. Be the first!
                </p>
            </div>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useBlogStore } from '@/stores/blog'
import { useAuthStore } from '@/stores/auth'
import { useFollowStore } from '@/stores/follow' // <--- IMPORT

const route = useRoute()
const router = useRouter()
const blogStore = useBlogStore()
const authStore = useAuthStore()
const followStore = useFollowStore() // <--- INIT

const blog = ref(null)
const newComment = ref('')
const isFollowingAuthor = ref(false) // <--- STATE

onMounted(async () => {
    const id = route.params.id
    try {
        // 1. Učitaj blog
        blog.value = await blogStore.getBlog(id)

        // 2. Proveri da li pratimo autora (ako nismo mi taj autor)
        if (authStore.isAuthenticated && authStore.user.id !== blog.value.authorId) {
            isFollowingAuthor.value = await followStore.isFollowing(blog.value.authorId)
        }
    } catch (e) {
        console.error("Failed to load data", e)
    }
})

// Logika: Mogu da komentarišem ako sam Autor ILI ako Pratim autora
const canComment = computed(() => {
    if (!authStore.isAuthenticated) return false;
    if (authStore.user.id === blog.value?.authorId) return true; // Mogu sebi da pišem
    return isFollowingAuthor.value; // Mogu ako pratim
})

const formattedBody = computed(() => {
    if (!blog.value?.description) return []
    return blog.value.description.split('\n')
})

const postComment = async () => {
    if (!newComment.value.trim()) return
    try {
        await blogStore.addComment(blog.value.id, newComment.value)
        blog.value = await blogStore.getBlog(blog.value.id) // Refresh
        newComment.value = ''
    } catch (e) {
        alert("Failed to post comment")
    }
}

const goToProfile = (userId) => {
    router.push(`/profile/${userId}`)
}
</script>

<style scoped>
.container { max-width: 800px; margin: 0 auto; padding-bottom: 50px; }
.text-center { text-align: center; margin-top: 50px; font-size: 1.2rem; color: #666; }

.btn-back { background: none; border: none; cursor: pointer; color: #666; display: flex; align-items: center; gap: 5px; margin-bottom: 20px; font-size: 1rem; }
.btn-back:hover { color: #cc072a; }

.hero-image img { width: 100%; max-height: 400px; object-fit: cover; border-radius: 12px; margin-bottom: 30px; }

.title { font-size: 2.5rem; color: #2c3e50; margin-bottom: 15px; line-height: 1.2; }

.meta-info { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; color: #777; }
.author-section { display: flex; align-items: center; gap: 10px; cursor: pointer; transition: 0.2s; }
.author-section:hover { color: #cc072a; }
.avatar { width: 40px; height: 40px; background: #cc072a; color: white; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: bold; }
.author-name { font-weight: 600; font-size: 1.1rem; }

.blog-body { font-size: 1.1rem; line-height: 1.8; color: #333; margin: 30px 0; }
.blog-body p { margin-bottom: 15px; }

/* KOMENTARI */
.comments-section { margin-top: 40px; }
.comment-form textarea { width: 100%; padding: 15px; border: 1px solid #ddd; border-radius: 8px; margin-bottom: 10px; font-family: inherit; resize: vertical; }
.btn-primary { background: #cc072a; color: white; border: none; padding: 10px 20px; border-radius: 6px; cursor: pointer; font-weight: bold; }
.btn-primary:disabled { background: #ccc; cursor: not-allowed; }

.comments-list { margin-top: 30px; }
.comment { background: #f9f9f9; padding: 15px; border-radius: 8px; margin-bottom: 15px; border: 1px solid #eee; }
.comment-header { display: flex; justify-content: space-between; margin-bottom: 8px; font-size: 0.9rem; }
.clickable { cursor: pointer; color: #2c3e50; }
.clickable:hover { color: #cc072a; text-decoration: underline; }
.comment-date { color: #999; }
.no-comments { text-align: center; color: #aaa; font-style: italic; margin-top: 20px; }
.login-msg { background: #f0f2f5; padding: 15px; border-radius: 8px; text-align: center; margin-bottom: 20px; }
.login-msg a { color: #cc072a; font-weight: bold; text-decoration: none; }

.container { max-width: 800px; margin: 0 auto; padding-bottom: 50px; }
.text-center { text-align: center; margin-top: 50px; color: #666; }
.btn-back { background: none; border: none; cursor: pointer; color: #666; margin-bottom: 20px; font-size: 1rem; }
.hero-image img { width: 100%; max-height: 400px; object-fit: cover; border-radius: 12px; margin-bottom: 30px; }
.title { font-size: 2.5rem; color: #2c3e50; margin-bottom: 15px; }

/* Autor sekcija */
.meta-info { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; color: #777; }
.author-section { display: flex; align-items: center; gap: 10px; cursor: pointer; }
.avatar { width: 40px; height: 40px; background: #cc072a; color: white; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: bold; overflow: hidden; }
.avatar-img { width: 100%; height: 100%; object-fit: cover; }
.author-text { display: flex; flex-direction: column; line-height: 1.2; }
.author-name { font-weight: 600; font-size: 1.1rem; }
.following-badge { font-size: 0.7rem; background: #eef; color: #44a; padding: 2px 6px; border-radius: 4px; display: inline-block; width: fit-content;}

.blog-body { font-size: 1.1rem; line-height: 1.8; color: #333; margin: 30px 0; }
.comments-section { margin-top: 40px; }
.comment-form textarea { width: 100%; padding: 15px; border: 1px solid #ddd; border-radius: 8px; margin-bottom: 10px; font-family: inherit; }
.btn-primary { background: #cc072a; color: white; border: none; padding: 10px 20px; border-radius: 6px; cursor: pointer; font-weight: bold; }

/* LOCKED STATE */
.locked-comments { text-align: center; background: #f8f9fa; padding: 30px; border-radius: 12px; border: 1px dashed #ccc; color: #555; }
.locked-comments i { font-size: 2rem; color: #777; margin-bottom: 10px; }
.btn-outline { background: white; border: 1px solid #cc072a; color: #cc072a; padding: 8px 16px; border-radius: 20px; cursor: pointer; font-weight: 600; margin-top: 10px; }
.btn-outline:hover { background: #cc072a; color: white; }

/* KOMENTARI LISTA */
.comments-list { margin-top: 30px; }
.comment { background: #f9f9f9; padding: 15px; border-radius: 8px; margin-bottom: 15px; border: 1px solid #eee; }
.comment-header { display: flex; justify-content: space-between; margin-bottom: 8px; font-size: 0.9rem; }
.comment-author { display: flex; align-items: center; gap: 8px; cursor: pointer; color: #2c3e50; }
.comment-author:hover { color: #cc072a; }
.small-avatar { width: 24px; height: 24px; border-radius: 50%; background: #ccc; color: white; display: flex; align-items: center; justify-content: center; font-size: 0.7rem; overflow: hidden; }
.small-avatar img { width: 100%; height: 100%; object-fit: cover; }
.comment-date { color: #999; }
.login-msg { background: #f0f2f5; padding: 15px; border-radius: 8px; text-align: center; margin-bottom: 20px; }
</style>
