document.addEventListener('DOMContentLoaded', function(){
    var input = document.getElementById('CoverImageFile');
    if (!input) return;
    input.addEventListener('change', function(e){
        var file = e.target.files && e.target.files[0];
        var previewWrap = document.getElementById('imagePreview');
        var previewImg = document.getElementById('preview');
        if (!file){ if (previewWrap) previewWrap.style.display = 'none'; return; }
        var validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
        if (validTypes.indexOf(file.type) === -1){
            appModal.alert({ title: 'Invalid File Type', message: 'Please upload only JPEG, PNG, or GIF images.', variant: 'error' });
            this.value = '';
            if (previewWrap) previewWrap.style.display = 'none';
            return;
        }
        if (file.size > 5 * 1024 * 1024){
            appModal.alert({ title: 'File Too Large', message: 'File size must be less than 5MB.', variant: 'warning' });
            this.value = '';
            if (previewWrap) previewWrap.style.display = 'none';
            return;
        }
        var reader = new FileReader();
        reader.onload = function(ev){
            if (previewImg) previewImg.src = ev.target.result;
            if (previewWrap) previewWrap.style.display = 'block';
        };
        reader.readAsDataURL(file);
    });
});
