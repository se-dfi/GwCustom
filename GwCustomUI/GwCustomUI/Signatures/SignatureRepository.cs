using GwCustomUI.Classes;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace GwCustomUI.Signatures {
    public class SignatureRepository {
        private readonly List<SignatureUser> _users = [];
        private readonly List<SignatureItem> _signatures = [];
        private readonly List<SignatureLayout> _layouts = [];

        public void AddUser(SignatureUser user) {
            if (!_users.Any(u => u.Login.Equals(user.Login, StringComparison.OrdinalIgnoreCase))) {
                _users.Add(user);
            }
        }

        public SignatureUser? GetUserByLogin(string login) {
            return _users.FirstOrDefault(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase));
        }

        public IReadOnlyList<SignatureUser> GetAllUsers() => _users;

        public void AddSignature(SignatureItem signature) {
            _signatures.Add(signature);
        }

        public SignatureItem? GetSignatureByIdAndUser(int id, SignatureUser user) {
            return _signatures.FirstOrDefault(s => s.Id == id && s.Users.Contains(user));
        }

        public IReadOnlyList<SignatureItem> GetAllSignatures() => _signatures;

        public void AddLayout(SignatureLayout layout) {
            if (!_layouts.Any(l => l.Id == layout.Id)) {
                _layouts.Add(layout);
            }
        }

        public SignatureLayout? GetLayoutById(int id) {
            return _layouts.FirstOrDefault(l => l.Id == id);
        }

        public IReadOnlyList<SignatureLayout> GetAllLayouts() => _layouts;

        public void ClearUsers() {
            _users.Clear();
        }

        public void ClearSignatures() {
            _signatures.Clear();
        }

        public void ClearLayouts() {
            _layouts.Clear();
        }
    }
}