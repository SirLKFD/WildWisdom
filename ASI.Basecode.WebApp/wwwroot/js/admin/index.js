// Pagination helper functions
function changePageSize(newPageSize) {
    var url = new URL(window.location);
    url.searchParams.set('pageSize', newPageSize);
    url.searchParams.set('page', '1');
    window.location.href = url.toString();
}

function goToPage(event) {
    if (event.key === 'Enter') {
        var pageInput = event.target;
        var totalPages = parseInt(pageInput.getAttribute('max') || '1');
        var page = Math.max(1, Math.min(parseInt(pageInput.value) || 1, totalPages));
        var url = new URL(window.location);
        url.searchParams.set('page', page);
        window.location.href = url.toString();
    }
}

// Image loading optimization and interactions
document.addEventListener('DOMContentLoaded', function() {
    const styledButtons = document.querySelectorAll('a[style*="linear-gradient"], button[style*="linear-gradient"]');
    styledButtons.forEach(function(button){
        button.addEventListener('mouseenter', function(){
            this.style.transform = 'translateY(-2px)';
            this.style.boxShadow = this.style.boxShadow.replace('0 2px 5px', '0 5px 15px').replace('0 3px 10px', '0 5px 20px');
        });
        button.addEventListener('mouseleave', function(){
            this.style.transform = 'translateY(0)';
            this.style.boxShadow = this.style.boxShadow.replace('0 5px 15px', '0 2px 5px').replace('0 5px 20px', '0 3px 10px');
        });
    });

    const imageObserver = new IntersectionObserver(function(entries, observer){
        entries.forEach(function(entry){
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
    lazyImages.forEach(function(img){ imageObserver.observe(img); });

    const preloadImages = function(){
        const visibleImages = document.querySelectorAll('img[src]:not([data-preloaded])');
        visibleImages.forEach(function(img){
            if (img.complete) {
                img.style.opacity = '1';
                img.classList.remove('image-loading');
            } else {
                img.addEventListener('load', function(){
                    this.style.opacity = '1';
                    this.classList.remove('image-loading');
                });
            }
            img.setAttribute('data-preloaded', 'true');
        });
    };
    setTimeout(preloadImages, 100);
});

// Book management actions (require appModal)
function deleteBook(id, title) {
    console.log('deleteBook called with', id, title);
    appModal.confirm({ title: 'Delete Book', message: 'Are you sure you want to delete "' + title + '"?', okText: 'Delete', cancelText: 'Cancel', variant: 'warning' }).then(function(ok) {
        console.log('deleteBook confirm result:', ok);
        if (!ok) return;
        document.getElementById('deleteBookId').value = id;
        document.getElementById('deleteForm').action = '/AdminHome/DeleteBook';
        console.log('Submitting delete form');
        document.getElementById('deleteForm').submit();
    });
}

function toggleStatus(id, title, willActivate) {
    console.log('toggleStatus called with', id, title, willActivate);
    var action = willActivate ? 'activate' : 'deactivate';
    var message = 'Are you sure you want to ' + action + ' "' + title + '"?\n\n';
    if (willActivate) { message += 'This will make the book visible and available for borrowing.'; }
    else { message += 'This will hide the book and prevent it from being borrowed.'; }

    appModal.confirm({ title: 'Please Confirm', message: message, okText: willActivate ? 'Activate' : 'Deactivate', cancelText: 'Cancel', variant: 'confirm' }).then(function(ok) {
        console.log('toggleStatus confirm result:', ok);
        if (!ok) return;
        document.getElementById('toggleBookId').value = id;
        document.getElementById('toggleForm').action = '/AdminHome/ToggleBookStatus';
        console.log('Submitting toggle form');
        document.getElementById('toggleForm').submit();
    });
}
