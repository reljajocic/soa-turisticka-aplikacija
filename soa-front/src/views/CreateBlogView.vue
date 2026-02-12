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
          <textarea
            v-model="form.description"
            rows="8"
            placeholder="Share your experience..."
            required
          ></textarea>
          <small class="hint">Supports: # headings, **bold**, *italic*, lists, links...</small>
        </div>

        <!-- PREVIEW -->
        <div class="preview">
          <div class="preview-header">Preview</div>
          <div class="preview-body markdown" v-html="renderedDescription"></div>
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
import { ref, computed } from "vue"
import { useRouter } from "vue-router"
import { useBlogStore } from "@/stores/blog"

import MarkdownIt from "markdown-it"
import DOMPurify from "dompurify"

const blogStore = useBlogStore()
const router = useRouter()

const form = ref({
  title: "",
  description: "",
  imageUrls: []
})
const imageInput = ref("")

// Markdown parser (no HTML allowed in Markdown input)
const md = new MarkdownIt({
  html: false,
  linkify: true,
  breaks: true
})

const renderedDescription = computed(() => {
  const raw = form.value.description || ""
  const html = md.render(raw)
  return DOMPurify.sanitize(html)
})

const handleSubmit = async () => {
  if (imageInput.value) form.value.imageUrls.push(imageInput.value)

  try {
    await blogStore.createBlog(form.value)
    alert("Blog published successfully!")
    router.push("/blogs")
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

.hint { display: block; margin-top: 8px; color: #777; }

/* Preview */
.preview {
  margin: 10px 0 25px 0;
  border: 1px solid #eee;
  border-radius: 10px;
  overflow: hidden;
  background: #fafafa;
}
.preview-header {
  padding: 10px 12px;
  font-weight: 700;
  color: #2c3e50;
  border-bottom: 1px solid #eee;
  background: #fff;
}
.preview-body {
  padding: 14px 16px;
}

/* Minimal markdown styling */
.markdown :deep(h1) { font-size: 1.7rem; margin: 0.6rem 0; }
.markdown :deep(h2) { font-size: 1.4rem; margin: 0.6rem 0; }
.markdown :deep(p) { margin: 0.5rem 0; color: #333; line-height: 1.6; }
.markdown :deep(ul), .markdown :deep(ol) { padding-left: 1.2rem; margin: 0.5rem 0; }
.markdown :deep(a) { color: #cc072a; text-decoration: underline; }

.actions { display: flex; justify-content: space-between; margin-top: 30px; }
.btn { padding: 12px 25px; border-radius: 8px; font-weight: bold; cursor: pointer; border: none; }
.btn-primary { background: #cc072a; color: white; }
.btn-secondary { background: #eee; color: #333; }
</style>
