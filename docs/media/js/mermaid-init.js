// Mermaid initialization for diagrams with theme switching support
(function() {
  // Load Mermaid from CDN
  const script = document.createElement('script');
  script.type = 'module';
  script.textContent = `
    import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.esm.min.mjs';
    
    // Function to get theme config - using base theme with colors that work in both modes
    function getThemeConfig() {
      return {
        theme: 'base',
        themeVariables: {
          primaryColor: '#4a9eff',
          primaryTextColor: '#ffffff',
          primaryBorderColor: '#2d7dd2',
          lineColor: '#00bbff',
          secondaryColor: '#d0d7de',
          tertiaryColor: '#f6f8fa',
          mainBkg: '#d0d7de',
          secondBkg: '#f6f8fa',
          textColor: '#00bbff',
          labelTextColor: '#00bbff',
          noteBkgColor: '#ddf4ff',
          noteTextColor: '#0066cc',
          noteBorderColor: '#00bbff',
          signalColor: '#00ddff',
          signalTextColor: '#00ddff',
          activationBkgColor: '#eaeef2',
          activationBorderColor: '#00bbff',
          sequenceNumberColor: '#ffffff',
          actorTextColor: '#0066cc',
          actorBkg: '#d0d7de',
          actorBorder: '#00bbff',
          labelBoxBkgColor: '#d0d7de',
          labelBoxBorderColor: '#00bbff',
          loopTextColor: '#00bbff',
          altTextColor: '#00bbff'
        }
      };
    }
    
    // Initialize with current theme
    function initMermaid() {
      const config = getThemeConfig();
      mermaid.initialize({ 
        startOnLoad: true,
        ...config
      });
    }
    
    // Re-render all diagrams
    function reRenderDiagrams() {
      const diagrams = document.querySelectorAll('.mermaid');
      diagrams.forEach((diagram, index) => {
        const originalContent = diagram.getAttribute('data-original-content');
        if (originalContent) {
          diagram.innerHTML = originalContent;
          diagram.removeAttribute('data-processed');
        }
      });
      mermaid.contentLoaded();
    }
    
    // Store original content before first render
    function storeOriginalContent() {
      const diagrams = document.querySelectorAll('.mermaid:not([data-original-content])');
      diagrams.forEach(diagram => {
        diagram.setAttribute('data-original-content', diagram.textContent);
      });
    }
    
    // Initialize on load
    window.addEventListener('DOMContentLoaded', () => {
      storeOriginalContent();
      initMermaid();
    });
    
    // Listen for theme changes via MutationObserver for data-theme attribute
    const observer = netheme
    function initMermaid() {
      const config = getThemeConfig();
      mermaid.initialize({ 
        startOnLoad: true,
        ...config
      });
    }
    
    // Re-render all diagrams (kept for potential future use)
    function reRenderDiagrams() {
      const diagrams = document.querySelectorAll('.mermaid');
      diagrams.forEach((diagram, index) => {
        const originalContent = diagram.getAttribute('data-original-content');
        if (originalContent) {
          diagram.innerHTML = originalContent;
          diagram.removeAttribute('data-processed');
        }
      });
      mermaid.contentLoaded();
    }
    
    // Store original content before first render
    function storeOriginalContent() {
      const diagrams = document.querySelectorAll('.mermaid:not([data-original-content])');
      diagrams.forEach(diagram => {
        diagram.setAttribute('data-original-content', diagram.textContent);
      });
    }
    
    // Initialize on load
    window.addEventListener('DOMContentLoaded', () => {
      storeOriginalContent();
      initMermaid();
    });