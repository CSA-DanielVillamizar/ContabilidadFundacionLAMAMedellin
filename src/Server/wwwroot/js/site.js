// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

/**
 * Descarga un archivo desde una cadena base64
 * @param {string} fileName - Nombre del archivo a descargar
 * @param {string} contentType - Tipo MIME del archivo
 * @param {string} base64String - Contenido del archivo en base64
 */
window.downloadFile = function (fileName, contentType, base64String) {
    const linkElement = document.createElement('a');
    linkElement.setAttribute('href', `data:${contentType};base64,${base64String}`);
    linkElement.setAttribute('download', fileName);
    linkElement.style.display = 'none';
    
    document.body.appendChild(linkElement);
    linkElement.click();
    document.body.removeChild(linkElement);
};

    /**
     * Descarga un archivo desde base64 (versión simplificada para PDFs)
     * @param {string} base64String - Contenido del archivo en base64
     * @param {string} fileName - Nombre del archivo a descargar
     */
    window.downloadFileFromBase64 = function (base64String, fileName) {
        const linkElement = document.createElement('a');
        linkElement.setAttribute('href', `data:application/pdf;base64,${base64String}`);
        linkElement.setAttribute('download', fileName);
        linkElement.style.display = 'none';
    
        document.body.appendChild(linkElement);
        linkElement.click();
        document.body.removeChild(linkElement);
    };

/**
 * Gestor de tema claro/oscuro con persistencia en localStorage
 */
window.themeManager = {
    STORAGE_KEY: 'lama-theme-preference',
    
    /**
     * Obtiene el tema actual (localStorage > preferencia sistema > default 'light')
     * @returns {string} 'light' o 'dark'
     */
    getTheme: function() {
        // 1. Verificar localStorage
        const stored = localStorage.getItem(this.STORAGE_KEY);
        if (stored === 'light' || stored === 'dark') {
            this.applyTheme(stored);
            return stored;
        }
        
        // 2. Verificar preferencia del sistema
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            this.applyTheme('dark');
            return 'dark';
        }
        
        // 3. Default: light
        this.applyTheme('light');
        return 'light';
    },
    
    /**
     * Establece el tema y lo persiste en localStorage
     * @param {string} theme - 'light' o 'dark'
     */
    setTheme: function(theme) {
        if (theme !== 'light' && theme !== 'dark') {
            console.error('Tema inválido:', theme);
            return;
        }
        
        localStorage.setItem(this.STORAGE_KEY, theme);
        this.applyTheme(theme);
    },
    
    /**
     * Aplica el tema al documento HTML
     * @param {string} theme - 'light' o 'dark'
     */
    applyTheme: function(theme) {
        const html = document.documentElement;
        
        if (theme === 'dark') {
            html.classList.add('dark');
        } else {
            html.classList.remove('dark');
        }
    },
    
    /**
     * Inicializa el tema al cargar la página (antes de Blazor)
     */
    initializeEarly: function() {
        const theme = this.getTheme();
        this.applyTheme(theme);
    }
};

// Aplicar tema INMEDIATAMENTE al cargar (antes de render Blazor)
// Esto evita el "flash" de tema incorrecto
(function() {
    window.themeManager.initializeEarly();
})();

// DEBUG: Confirmar que site.js se cargó correctamente
console.log('✅ site.js cargado correctamente');
console.log('✅ downloadFileFromBase64 definido:', typeof window.downloadFileFromBase64);
