// wwwroot/js/site.js
window.blazorToast = (msg) => {
    const toast = document.createElement('div');
    toast.className = 'toast align-items-center text-bg-primary border-0 position-fixed bottom-0 end-0 m-3';
    toast.style.zIndex = 1050;
    toast.innerHTML = `<div class="d-flex">
        <div class="toast-body">${msg}</div>
        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
    </div>`;
    document.body.appendChild(toast);
    var bsToast = new bootstrap.Toast(toast, { delay: 3000 });
    bsToast.show();
    toast.addEventListener('hidden.bs.toast', () => toast.remove());
};
