document.addEventListener('DOMContentLoaded', function(){
    var fileInput = document.getElementById('profilePictureFile');
    if (fileInput){
        fileInput.addEventListener('change', function(e){
            var file = e.target.files && e.target.files[0];
            if (!file) return;
            var reader = new FileReader();
            reader.onload = function(ev){
                var preview = document.getElementById('profilePreview');
                if (preview){ preview.innerHTML = '<img src="' + ev.target.result + '" alt="Profile" class="profile-preview" />'; }
            };
            reader.readAsDataURL(file);
        });
    }
    var confirmInput = document.getElementById('confirmDeleteText');
    var confirmBtn = document.getElementById('confirmDeleteBtn');
    if (confirmInput && confirmBtn){
        confirmInput.addEventListener('input', function(){
            confirmBtn.disabled = this.value.trim() !== 'DELETE';
        });
    }
});
