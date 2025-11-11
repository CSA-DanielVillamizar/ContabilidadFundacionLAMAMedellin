// LamaCharts - Wrapper para Chart.js
// Gestiona instancias de gráficos y permite actualizaciones sin duplicados
(function(){
  const instances = {};

  function renderOrUpdate(canvasId, config){
    const ctx = document.getElementById(canvasId);
    if(!ctx) {
      console.warn(`⚠️ No se encontró el canvas con id: ${canvasId}`);
      return;
    }
    
    // Destruir instancia existente antes de crear nueva
    const existing = instances[canvasId];
    if(existing){ 
      existing.destroy(); 
    }
    
    // Crear nueva instancia
    instances[canvasId] = new Chart(ctx, config);
  }

  // Exponer API global
  window.LamaCharts = { renderOrUpdate };
})();
