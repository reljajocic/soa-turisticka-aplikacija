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
                <div class="avatar-container">
                    <img v-if="blog.authorAvatarUrl" :src="blog.authorAvatarUrl" class="avatar-img" />
                    <div v-else class="avatar-placeholder">
                        {{ blog.username.charAt(0).toUpperCase() }}
                    </div>
                </div>

                <div class="author-text">
                    <span class="author-name">@{{ blog.username }}</span>
                    <div class="sub-meta">
                        <span class="date">{{ new Date(blog.createdAt).toLocaleDateString() }}</span>
                        <span v-if="isFollowingAuthor" class="following-badge">Following</span>
                    </div>
                </div>
            </div>
        </div>

        <hr />
        
        <div class="blog-body">
            <p v-for="(paragraph, index) in formattedBody" :key="index">{{ paragraph }}</p>
        </div>
        
        <hr />

        <div class="comments-section">
            <h3>Comments ({{ blog.comments ? blog.comments.length : 0 }})</h3>

            <div v-if="canComment" class="comment-form">
                <textarea v-model="newComment" placeholder="What are your thoughts?" rows="3"></textarea>
                <button @click="postComment" class="btn btn-primary" :disabled="!newComment.trim()">
                    Post Comment
                </button>
            </div>

            <div v-else-if="authStore.isAuthenticated" class="locked-comments">
                <i class="fa fa-lock"></i>
                <p>Follow <strong>@{{ blog.username }}</strong> to join the discussion.</p>
                <button @click="goToProfile(blog.authorId)" class="btn btn-outline">
                    View Profile
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
                    No comments yet.
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
import { useFollowStore } from '@/stores/follow'

const route = useRoute()
const router = useRouter()
const blogStore = useBlogStore()
const authStore = useAuthStore()
const followStore = useFollowStore()

const blog = ref(null)
const newComment = ref('')
const isFollowingAuthor = ref(false)

onMounted(async () => {
    const id = route.params.id
    try {
        blog.value = await blogStore.getBlog(id)
        if (authStore.isAuthenticated && authStore.user.id !== blog.value.authorId) {
            isFollowingAuthor.value = await followStore.isFollowing(blog.value.authorId)
        }
    } catch (e) {
        console.error("Failed to load data", e)
    }
})

const canComment = computed(() => {
    if (!authStore.isAuthenticated) return false;
    if (authStore.user.id === blog.value?.authorId) return true;
    return isFollowingAuthor.value;
})

const formattedBody = computed(() => {
    if (!blog.value?.description) return []
    return blog.value.description.split('\n')
})

const postComment = async () => {
    if (!newComment.value.trim()) return
    try {
        await blogStore.addComment(blog.value.id, newComment.value)
        blog.value = await blogStore.getBlog(blog.value.id)
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
.text-center { text-align: center; margin-top: 50px; color: #666; }
.btn-back { background: none; border: none; cursor: pointer; color: #666; display: flex; align-items: center; gap: 5px; margin-bottom: 20px; font-size: 1rem; }
.btn-back:hover { color: #cc072a; }

.hero-image img { width: 100%; max-height: 400px; object-fit: cover; border-radius: 12px; margin-bottom: 30px; }
.title { font-size: 2.5rem; color: #111; margin-bottom: 20px; font-weight: 800; line-height: 1.2; }

/* AUTOR SEKCIJA (POBOLJŠANA) */
.meta-info { display: flex; align-items: center; margin-bottom: 25px; }
.author-section { display: flex; align-items: center; gap: 12px; cursor: pointer; }

/* Avatar stilovi */
.avatar-container { width: 48px; height: 48px; border-radius: 50%; overflow: hidden; border: 2px solid #eee; }
.avatar-img { width: 100%; height: 100%; object-fit: cover; }
.avatar-placeholder { width: 100%; height: 100%; background: #cc072a; color: white; display: flex; align-items: center; justify-content: center; font-size: 1.5rem; font-weight: bold; }

.author-text { display: flex; flex-direction: column; }
.author-name { font-weight: 700; font-size: 1.1rem; color: #222; }
.author-name:hover { text-decoration: underline; color: #cc072a; }

.sub-meta { display: flex; align-items: center; gap: 8px; font-size: 0.9rem; color: #777; }
.following-badge { font-size: 0.75rem; background: #e6fcf5; color: #0ca678; padding: 1px 6px; border-radius: 4px; font-weight: 600; border: 1px solid #0ca678; }

.blog-body { font-size: 1.2rem; line-height: 1.8; color: #292929; margin: 30px 0; font-family: 'Georgia', serif; } /* Lepši font za čitanje */
.blog-body p { margin-bottom: 20px; }

/* Komentari (čistiji stil) */
.comments-section { margin-top: 40px; }
.comment-form textarea { width: 100%; padding: 15px; border: 1px solid #ddd; border-radius: 8px; margin-bottom: 10px; font-family: inherit; resize: vertical; box-sizing: border-box; }
.btn-primary { background: #cc072a; color: white; border: none; padding: 10px 20px; border-radius: 20px; cursor: pointer; font-weight: bold; }
.btn-primary:disabled { background: #eee; color: #aaa; cursor: not-allowed; }

.locked-comments { text-align: center; background: #f8f9fa; padding: 40px; border-radius: 12px; color: #555; }
.locked-comments i { font-size: 1.5rem; margin-bottom: 10px; color: #777; }
.btn-outline { background: white; border: 1px solid #cc072a; color: #cc072a; padding: 6px 16px; border-radius: 20px; cursor: pointer; font-weight: 600; margin-top: 10px; }
.btn-outline:hover { background: #fff0f0; }

.comments-list { margin-top: 30px; }
.comment { padding: 15px 0; border-bottom: 1px solid #eee; }
.comment-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 6px; }
.comment-author { display: flex; align-items: center; gap: 8px; font-weight: 600; font-size: 0.95rem; cursor: pointer; }
.small-avatar { width: 28px; height: 28px; border-radius: 50%; overflow: hidden; background: #eee; display: flex; align-items: center; justify-content: center; }
.small-avatar img { width: 100%; height: 100%; object-fit: cover; }
.comment-date { font-size: 0.8rem; color: #999; }
.login-msg { background: #f0f2f5; padding: 20px; text-align: center; border-radius: 8px; }
</style>