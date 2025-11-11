// Simple helper to render/update charts by element id using Chart.js
(function(){
  const instances = {};

  function renderOrUpdate(canvasId, config){
    const ctx = document.getElementById(canvasId);
    if(!ctx) return;
    const existing = instances[canvasId];
    if(existing){ existing.destroy(); }
    instances[canvasId] = new Chart(ctx, config);
  }

  window.LamaCharts = { renderOrUpdate };
})();
