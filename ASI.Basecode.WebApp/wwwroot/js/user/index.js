document.addEventListener('DOMContentLoaded', function() {
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                if (img.dataset.src) {
                    img.src = img.dataset.src;
                    img.removeAttribute('data-src');
                    observer.unobserve(img);
                }
            }
        });
    }, { rootMargin: '50px 0px', threshold: 0.01 });

    const lazyImages = document.querySelectorAll('img[loading="lazy"]');
    lazyImages.forEach(img => { imageObserver.observe(img); });

    const preloadImages = () => {
        const visibleImages = document.querySelectorAll('img[src]:not([data-preloaded])');
        visibleImages.forEach(img => {
            if (img.complete) {
                img.style.opacity = '1';
                img.classList.remove('image-loading');
            } else {
                img.addEventListener('load', function() {
                    this.style.opacity = '1';
                    this.classList.remove('image-loading');
                });
            }
            img.setAttribute('data-preloaded', 'true');
        });
    };
    setTimeout(preloadImages, 100);
});

var gotoInput = document.getElementById('gotoPageInput');
if (gotoInput) {
    gotoInput.addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            var totalPages = parseInt(this.getAttribute('max') || '1');
            var pageSize = this.getAttribute('data-page-size') || '25';
            var search = this.getAttribute('data-search') || '';
            var category = this.getAttribute('data-category') || '';
            var genre = this.getAttribute('data-genre') || '';
            var availability = this.getAttribute('data-availability') || '';
            const page = Math.max(1, Math.min(parseInt(this.value) || 1, totalPages));
            window.location.href = '?page=' + page + '&pageSize=' + pageSize + '&search=' + encodeURIComponent(search) + '&category=' + encodeURIComponent(category) + '&genre=' + encodeURIComponent(genre) + '&availability=' + encodeURIComponent(availability);
        }
    });
}
