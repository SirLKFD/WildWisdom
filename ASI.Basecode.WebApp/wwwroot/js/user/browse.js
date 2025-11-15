(function(){
    function toggleCategory(categoryName){
        var categoryTiles = document.getElementById('category-' + categoryName);
        if (categoryTiles){ categoryTiles.style.display = categoryTiles.style.display === 'none' ? 'grid' : 'none'; }
    }
    window.toggleCategory = toggleCategory;

    function filterByCategory(category){
        updateActiveFilter(category);
        var container = document.querySelector('.start-menu-container');
        var browseUrl = (container && container.dataset.browseUrl) || '/UserHome/Browse';
        window.location.href = browseUrl + '?category=' + encodeURIComponent(category);
    }
    window.filterByCategory = filterByCategory;

    function filterByNew(){
        updateActiveFilter('new');
        var container = document.querySelector('.start-menu-container');
        var browseUrl = (container && container.dataset.browseUrl) || '/UserHome/Browse';
        window.location.href = browseUrl + '?category=new-arrivals';
    }
    window.filterByNew = filterByNew;

    function clearAllFilters(){
        document.querySelectorAll('.filter-btn').forEach(function(btn){ btn.classList.remove('active'); });
        var container = document.querySelector('.start-menu-container');
        var browseUrl = (container && container.dataset.browseUrl) || '/UserHome/Browse';
        window.location.href = browseUrl;
    }
    window.clearAllFilters = clearAllFilters;

    function updateActiveFilter(filterType){
        document.querySelectorAll('.filter-btn').forEach(function(btn){ btn.classList.remove('active'); });
        var buttonId = filterType === 'Fiction' ? 'filter-fiction' : filterType === 'Non-Fiction' ? 'filter-nonfiction' : filterType === 'new' ? 'filter-new' : null;
        if (buttonId){ var el = document.getElementById(buttonId); if (el) el.classList.add('active'); }
    }
    window.updateActiveFilter = updateActiveFilter;

    function updateNavigationState(){ /* no-op placeholder if used elsewhere */ }
    window.updateNavigationState = window.updateNavigationState || updateNavigationState;

    function initializeActiveFilters(){
        var container = document.querySelector('.start-menu-container');
        var currentCategory = (container && container.dataset.currentCategory) || '';
        document.querySelectorAll('.filter-btn').forEach(function(btn){ btn.classList.remove('active'); });
        if (currentCategory === 'Fiction'){ var f=document.getElementById('filter-fiction'); if (f) f.classList.add('active'); }
        else if (currentCategory === 'Non-Fiction'){ var n=document.getElementById('filter-nonfiction'); if (n) n.classList.add('active'); }
        else if (currentCategory === 'new-arrivals'){ var nn=document.getElementById('filter-new'); if (nn) nn.classList.add('active'); }
    }

    document.addEventListener('DOMContentLoaded', function(){
        initializeActiveFilters();
        if (typeof window.updateNavigationState === 'function') window.updateNavigationState();
    });
})();
