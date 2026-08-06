/* =============================================================
   Generador de Partículas Doradas — LuxuryCo
   Uso: <div class="hero-canvas" id="capa-particulas"></div>
        dentro de cualquier sección hero, luego llamar iniciarParticulas()
   ============================================================= */

function iniciarParticulas(idContenedor = "capa-particulas", cantidad = 30) {
    const capa = document.getElementById(idContenedor);
    if (!capa) return;

    capa.innerHTML = "";

    for (let i = 0; i < cantidad; i++) {
        const particula = document.createElement("span");
        particula.className = "particula-dorada";

        // Tamaño aleatorio entre 2px y 5px
        const tamano = 2 + Math.random() * 3;
        particula.style.width  = tamano + "px";
        particula.style.height = tamano + "px";

        // Posición horizontal aleatoria
        particula.style.left = Math.random() * 100 + "%";

        // Deriva horizontal durante la animación (de -40px a +40px)
        particula.style.setProperty("--deriva", (Math.random() * 80 - 40) + "px");

        // Duración y retraso de animación aleatorios para que no sean sincronizadas
        particula.style.animationDuration = (7 + Math.random() * 8) + "s";
        particula.style.animationDelay    = (Math.random() * 8) + "s";

        capa.appendChild(particula);
    }
}

// Inicializar automáticamente al cargar la página
document.addEventListener("DOMContentLoaded", () => iniciarParticulas());
