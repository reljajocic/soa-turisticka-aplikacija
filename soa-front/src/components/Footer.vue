<template>
  <footer class="app-footer">
    <div class="container">
      <div class="footer-content">
        
        <div class="footer-brand">
          <h2><img src="@/assets/travela_icon.png" alt="Travela Logo" class="footer-logo">
          Travela</h2>
          <p>Your journey begins here.</p>
          <p class="small-text">Best tours, best memories.</p>
        </div>

        <div class="footer-links">
          <h3>Quick Links</h3>
          <ul>
            <li><router-link to="/">Home</router-link></li>
            <li><router-link to="/tours">Tours</router-link></li>
            <li><router-link to="/blogs">Blogs</router-link></li>
            <li><router-link to="/simulator">Simulator</router-link></li>
            <li><router-link to="/my-tours">My Tours</router-link></li>
          </ul>
        </div>

      </div>

      <div class="footer-bottom">
        <p>&copy; 2026 <strong>Travela</strong>. All Rights Reserved.</p>
      </div>
    </div>
  </footer>
</template>

<style scoped>
.app-footer {
    background-color: #cc072a; /* Naša glavna crvena boja */
    color: white;
    padding: 40px 0 20px 0;
    margin-top: auto; /* Ovo gura footer na dno ako je stranica kratka */
}

.footer-content {
    display: flex;
    justify-content: space-between;
    flex-wrap: wrap;
    gap: 30px;
    margin-bottom: 30px;
    border-bottom: 1px solid rgba(255, 255, 255, 0.2);
    padding-bottom: 30px;
}

.footer-logo {
    height: 25px; /* Prilagodi visinu ako treba */
    filter: brightness(0) invert(1); 
}

/* Brand sekcija */
.footer-brand h2 {
    font-size: 2rem;
    margin: 0 0 10px 0;
    font-weight: 800;
    letter-spacing: 1px;
}
.footer-brand p {
    margin: 5px 0;
    opacity: 0.9;
}
.small-text {
    font-size: 0.85rem;
    opacity: 0.7;
}

/* Linkovi */
.footer-links h3 {
    font-size: 1.2rem;
    margin-bottom: 15px;
    border-bottom: 2px solid white;
    display: inline-block;
    padding-bottom: 5px;
}

.footer-links ul {
    list-style: none;
    padding: 0;
    margin: 0;
    display: flex;
    flex-wrap: wrap; /* Da se prebace u novi red na malim ekranima */
    gap: 20px;
}

.footer-links a {
    color: white;
    text-decoration: none;
    font-weight: 500;
    font-size: 1rem;
    transition: opacity 0.2s;
}

.footer-links a:hover {
    opacity: 0.7;
    text-decoration: underline;
}

/* Bottom Copyright */
.footer-bottom {
    text-align: center;
    font-size: 0.9rem;
    opacity: 0.8;
}

/* Responsive */
@media (max-width: 600px) {
    .footer-content {
        flex-direction: column;
        text-align: center;
    }
    .footer-links ul {
        justify-content: center;
        flex-direction: column;
        gap: 10px;
    }
}
</style>