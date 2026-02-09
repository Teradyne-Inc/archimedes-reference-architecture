document.addEventListener("DOMContentLoaded", function () {
  document.querySelectorAll("pre code").forEach(function (codeBlock) {
    const button = document.createElement("button");
    button.innerHTML = "📋 Copy";
    button.className = "copy-button";

    button.addEventListener("click", () => {
      navigator.clipboard.writeText(codeBlock.innerText);
      button.innerHTML = "✅ Copied!";
      setTimeout(() => (button.innerHTML = "📋 Copy"), 2000);
    });

    const pre = codeBlock.parentNode;
    pre.style.position = "relative";
    pre.appendChild(button);
  });
});
