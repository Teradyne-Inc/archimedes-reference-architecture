// Mermaid initialization for diagrams
(function() {
  // Load Mermaid from CDN
  const script = document.createElement('script');
  script.type = 'module';
  script.textContent = `
    import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.esm.min.mjs';
    mermaid.initialize({ 
      startOnLoad: true,
      theme: 'default',
      themeVariables: {
        primaryColor: '#23579F',
        primaryTextColor: '#fff',
        primaryBorderColor: '#23579F',
        lineColor: '#23579F',
        secondaryColor: '#006100',
        tertiaryColor: '#fff'
      }
    });
  `;
  document.head.appendChild(script);
})();
